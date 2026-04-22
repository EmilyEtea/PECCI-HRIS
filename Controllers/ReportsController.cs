using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;

namespace PECCI_HRIS.Controllers
{
    [Authorize(Roles = "HR Admin,HR Staff,Manager")]
    public class ReportsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        // ── Employee List Report ──────────────────────────────────────────────────
        public async Task<IActionResult> EmployeeList(string? status, int? departmentId)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status == status);

            if (departmentId.HasValue)
                query = query.Where(e => e.DepartmentID == departmentId);

            var employees = await query.OrderBy(e => e.LastName).ToListAsync();

            ViewBag.Departments  = await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Status       = status;
            ViewBag.DepartmentId = departmentId;
            ViewBag.GeneratedAt  = DateTime.Now;

            return View(employees);
        }

        // ── Attendance Summary Report ─────────────────────────────────────────────
        public async Task<IActionResult> AttendanceSummary(int? month, int? year, int? departmentId)
        {
            month ??= DateTime.Today.Month;
            year  ??= DateTime.Today.Year;

            var startDate = new DateTime(year.Value, month.Value, 1);
            var endDate   = startDate.AddMonths(1).AddDays(-1);

            var query = _context.AttendanceRecords
                .Include(a => a.Employee).ThenInclude(e => e!.Department)
                .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate);

            if (departmentId.HasValue)
                query = query.Where(a => a.Employee!.DepartmentID == departmentId);

            var records = await query.ToListAsync();

            var summary = records
                .GroupBy(a => a.EmployeeID)
                .Select(g => new
                {
                    Employee             = g.First().Employee!,
                    TotalPresent         = g.Count(r => r.AttendanceStatus == "Present"),
                    TotalLate            = g.Count(r => r.AttendanceStatus == "Late"),
                    TotalAbsent          = g.Count(r => r.AttendanceStatus == "Absent"),
                    TotalOnLeave         = g.Count(r => r.AttendanceStatus == "On Leave"),
                    TotalLateMinutes     = g.Sum(r => r.LateMinutes ?? 0),
                    TotalOvertimeMinutes = g.Sum(r => r.OvertimeMinutes ?? 0),
                    TotalHoursWorked     = g.Sum(r => r.TotalHoursWorked ?? 0)
                })
                .OrderBy(s => s.Employee.LastName)
                .ToList();

            ViewBag.Month        = month;
            ViewBag.Year         = year;
            ViewBag.MonthName    = startDate.ToString("MMMM yyyy");
            ViewBag.DepartmentId = departmentId;
            ViewBag.Departments  = await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.GeneratedAt  = DateTime.Now;

            return View(summary);
        }

        // ── Leave Summary Report ──────────────────────────────────────────────────
        public async Task<IActionResult> LeaveSummary(int? year, int? departmentId)
        {
            year ??= DateTime.Today.Year;

            var query = _context.LeaveApplications
                .Include(l => l.Employee).ThenInclude(e => e!.Department)
                .Include(l => l.LeaveType)
                .Where(l => l.StartDate.Year == year);

            if (departmentId.HasValue)
                query = query.Where(l => l.Employee!.DepartmentID == departmentId);

            var applications = await query.OrderBy(l => l.Employee!.LastName).ToListAsync();

            ViewBag.Year         = year;
            ViewBag.DepartmentId = departmentId;
            ViewBag.Departments  = await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.GeneratedAt  = DateTime.Now;

            return View(applications);
        }

        // ── Payroll Summary Report ────────────────────────────────────────────────
        public async Task<IActionResult> PayrollSummary(int? month, int? year)
        {
            month ??= DateTime.Today.Month;
            year  ??= DateTime.Today.Year;

            var records = await _context.PayrollRecords
                .Include(p => p.Employee).ThenInclude(e => e!.Department)
                .Where(p => p.PeriodStart.Year == year && p.PeriodStart.Month == month)
                .OrderBy(p => p.Employee!.LastName)
                .ToListAsync();

            ViewBag.Month       = month;
            ViewBag.Year        = year;
            ViewBag.MonthName   = new DateTime(year.Value, month.Value, 1).ToString("MMMM yyyy");
            ViewBag.GeneratedAt = DateTime.Now;
            ViewBag.TotalGross  = records.Sum(r => r.StoredGrossPay);
            ViewBag.TotalNet    = records.Sum(r => r.StoredNetPay);
            ViewBag.TotalDeductions = records.Sum(r => r.StoredTotalDeductions);

            return View(records);
        }
    }
}
