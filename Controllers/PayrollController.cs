using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;
using PECCI_HRIS.Models;
using PECCI_HRIS.Services;
using PECCI_HRIS.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PECCI_HRIS.Controllers
{
    [Authorize(Roles = "HR Admin,HR Staff")]
    public class PayrollController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly TaxComputationService _taxService;
        private readonly AttendanceComputationService _attendanceService;
        private readonly AuditService _auditService;

        public PayrollController(ApplicationDbContext context,
            TaxComputationService taxService,
            AttendanceComputationService attendanceService,
            AuditService auditService)
        {
            _context = context;
            _taxService = taxService;
            _attendanceService = attendanceService;
            _auditService = auditService;
        }

        // ── Payroll List ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(int? month, int? year, string? status)
        {
            month ??= DateTime.Today.Month;
            year  ??= DateTime.Today.Year;

            var query = _context.PayrollRecords
                .Include(p => p.Employee).ThenInclude(e => e!.Department)
                .Where(p => p.PeriodStart.Year == year && p.PeriodStart.Month == month);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            var records = await query.OrderBy(p => p.Employee!.LastName).ToListAsync();

            ViewBag.Month = month;
            ViewBag.Year  = year;
            ViewBag.Status = status;

            return View(records);
        }

        // ── Compute Payroll ───────────────────────────────────────────────────────
        public async Task<IActionResult> Compute()
        {
            var vm = new PayrollComputeViewModel
            {
                Month  = DateTime.Today.Month,
                Year   = DateTime.Today.Year,
                Employees = await _context.Employees
                    .Where(e => e.Status == "Active")
                    .OrderBy(e => e.LastName)
                    .Select(e => new SelectListItem
                    {
                        Value = e.EmployeeID.ToString(),
                        Text  = $"{e.FullName} ({e.EmployeeNo})"
                    })
                    .ToListAsync()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Compute(PayrollComputeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Employees = await GetEmployeeList();
                return View(vm);
            }

            // Determine period dates
            bool isFirstCutoff = vm.CutoffPeriod == "1-15";
            var periodStart = isFirstCutoff
                ? new DateTime(vm.Year, vm.Month, 1)
                : new DateTime(vm.Year, vm.Month, 16);
            var periodEnd = isFirstCutoff
                ? new DateTime(vm.Year, vm.Month, 15)
                : new DateTime(vm.Year, vm.Month, DateTime.DaysInMonth(vm.Year, vm.Month));

            string payPeriod = $"{vm.Year}-{vm.Month:D2}-{vm.CutoffPeriod}";

            // Get employees to compute
            var employees = vm.EmployeeID.HasValue
                ? await _context.Employees.Include(e => e.Position)
                    .Where(e => e.EmployeeID == vm.EmployeeID && e.Status == "Active").ToListAsync()
                : await _context.Employees.Include(e => e.Position)
                    .Where(e => e.Status == "Active").ToListAsync();

            int computed = 0;
            foreach (var emp in employees)
            {
                // Skip if already computed
                bool exists = await _context.PayrollRecords
                    .AnyAsync(p => p.EmployeeID == emp.EmployeeID && p.PayPeriod == payPeriod);
                if (exists) continue;

                decimal basicSalary = (emp.Position?.BasicSalary ?? 0) / 2; // semi-monthly

                // Get attendance for period
                var attendance = await _context.AttendanceRecords
                    .Where(a => a.EmployeeID == emp.EmployeeID &&
                                a.AttendanceDate >= periodStart &&
                                a.AttendanceDate <= periodEnd)
                    .ToListAsync();

                double totalLateMinutes    = attendance.Sum(a => a.LateMinutes ?? 0);
                double totalOvertimeMinutes = attendance.Sum(a => a.OvertimeMinutes ?? 0);
                int daysAbsent = attendance.Count(a => a.AttendanceStatus == "Absent");
                int daysWorked = attendance.Count(a => a.AttendanceStatus != "Absent");

                // Compute deductions
                var govDeductions = _taxService.ComputeGovernmentDeductions(basicSalary * 2); // monthly basis
                decimal sss       = govDeductions.SSS / 2;
                decimal philHealth = govDeductions.PhilHealth / 2;
                decimal pagIbig   = govDeductions.PagIbig / 2;
                decimal tax       = govDeductions.WithholdingTax / 2;

                decimal lateDeduction      = _attendanceService.ComputeLateDeduction(totalLateMinutes, basicSalary * 2);
                decimal undertimeDeduction = _attendanceService.ComputeUndertimeDeduction(attendance.Sum(a => a.UndertimeMinutes ?? 0), basicSalary * 2);
                decimal overtimePay        = _attendanceService.ComputeOvertimePay(totalOvertimeMinutes, basicSalary * 2);

                // Absent deduction
                decimal dailyRate = (basicSalary * 2) / 22m;
                decimal absentDeduction = dailyRate * daysAbsent;

                // Custom deductions (loans, cash advances, etc.) from EmployeeDeductions table
                var customDeductions = await _context.EmployeeDeductions
                    .Where(d => d.EmployeeID == emp.EmployeeID
                             && d.Month == vm.Month
                             && d.Year == vm.Year
                             && d.CutoffPeriod == vm.CutoffPeriod
                             && d.Status == "Active")
                    .ToListAsync();
                decimal customDeductionTotal = customDeductions.Sum(d => d.Amount);

                var record = new PayrollRecord
                {
                    EmployeeID             = emp.EmployeeID,
                    PayPeriod              = payPeriod,
                    PeriodStart            = periodStart,
                    PeriodEnd              = periodEnd,
                    BasicSalary            = basicSalary,
                    OvertimePay            = overtimePay,
                    SSSContribution        = sss,
                    PhilHealthContribution = philHealth,
                    PagIbigContribution    = pagIbig,
                    WithholdingTax         = tax,
                    LateDeductions         = lateDeduction,
                    UndertimeDeductions    = undertimeDeduction,
                    OtherDeductions        = absentDeduction + customDeductionTotal,
                    DaysWorked             = daysWorked,
                    DaysAbsent             = daysAbsent,
                    TotalOvertimeHours     = totalOvertimeMinutes / 60.0,
                    TotalLateMinutes       = totalLateMinutes,
                    Status                 = "Draft",
                    CreatedAt              = DateTime.Now,
                    CreatedBy              = GetCurrentUserID()
                };

                // Store computed totals
                record.StoredGrossPay        = record.GrossPay;
                record.StoredTotalDeductions = record.TotalDeductions;
                record.StoredNetPay          = record.NetPay;

                _context.PayrollRecords.Add(record);

                // Mark custom deductions as Applied
                foreach (var d in customDeductions)
                    d.Status = "Applied";

                computed++;
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Compute", "Payroll",
                $"Computed payroll for {computed} employee(s) — Period: {payPeriod}",
                GetClientIP());

            TempData["Success"] = $"Payroll computed for {computed} employee(s). Period: {payPeriod}";
            return RedirectToAction(nameof(Index), new { month = vm.Month, year = vm.Year });
        }

        // ── Payslips ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Payslips(int? employeeId, int? month, int? year)
        {
            month ??= DateTime.Today.Month;
            year  ??= DateTime.Today.Year;

            var query = _context.PayrollRecords
                .Include(p => p.Employee).ThenInclude(e => e!.Department)
                .Include(p => p.Employee!.Position)
                .Where(p => p.PeriodStart.Year == year && p.PeriodStart.Month == month);

            if (employeeId.HasValue)
                query = query.Where(p => p.EmployeeID == employeeId);

            var records = await query.OrderBy(p => p.Employee!.LastName).ToListAsync();

            var summaries = records.Select(r => new PayslipViewModel
            {
                PayrollID              = r.PayrollID,
                EmployeeName           = r.Employee?.DisplayName ?? string.Empty,
                EmployeeNo             = r.Employee?.EmployeeNo ?? string.Empty,
                Department             = r.Employee?.Department?.DepartmentName ?? string.Empty,
                PayPeriod              = r.PayPeriod,
                BasicSalary            = r.BasicSalary,
                OvertimePay            = r.OvertimePay,
                HolidayPay             = r.HolidayPay,
                NightDifferential      = r.NightDifferential,
                Allowances             = r.Allowances,
                GrossPay               = r.StoredGrossPay,
                SSSContribution        = r.SSSContribution,
                PhilHealthContribution = r.PhilHealthContribution,
                PagIbigContribution    = r.PagIbigContribution,
                WithholdingTax         = r.WithholdingTax,
                LateDeductions         = r.LateDeductions,
                UndertimeDeductions    = r.UndertimeDeductions,
                OtherDeductions        = r.OtherDeductions,
                TotalDeductions        = r.StoredTotalDeductions,
                NetPay                 = r.StoredNetPay,
                DaysWorked             = r.DaysWorked,
                DaysAbsent             = r.DaysAbsent,
                TotalOvertimeHours     = r.TotalOvertimeHours,
                TotalLateMinutes       = r.TotalLateMinutes,
                Status                 = r.Status,
                TaxBracket             = _taxService.GetTaxBracketLabel(r.BasicSalary * 2 - r.SSSContribution * 2 - r.PhilHealthContribution * 2 - r.PagIbigContribution * 2)
            }).ToList();

            ViewBag.Month      = month;
            ViewBag.Year       = year;
            ViewBag.EmployeeId = employeeId;
            ViewBag.Employees  = await GetEmployeeList();

            return View(summaries);
        }

        // ── Finalize Payroll ──────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> Finalize(int id)
        {
            var record = await _context.PayrollRecords.FindAsync(id);
            if (record == null) return NotFound();

            record.Status      = "Finalized";
            record.FinalizedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payroll record finalized.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetEmployeeList() =>
            await _context.Employees
                .Where(e => e.Status == "Active")
                .OrderBy(e => e.LastName)
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeID.ToString(),
                    Text  = $"{e.FullName} ({e.EmployeeNo})"
                })
                .ToListAsync();
    }
}
