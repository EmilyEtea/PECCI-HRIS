using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;
using PECCI_HRIS.Models;
using PECCI_HRIS.Services;
using PECCI_HRIS.ViewModels;

namespace PECCI_HRIS.Controllers
{
    [Authorize]
    public class AttendanceController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly AttendanceComputationService _computeService;
        private readonly AuditService _auditService;

        public AttendanceController(ApplicationDbContext context,
            AttendanceComputationService computeService,
            AuditService auditService)
        {
            _context = context;
            _computeService = computeService;
            _auditService = auditService;
        }

        // ── List / History ───────────────────────────────────────────────────────

        public async Task<IActionResult> Index(int? employeeId, DateTime? dateFrom, DateTime? dateTo, string? status)
        {
            dateFrom ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dateTo   ??= DateTime.Today;

            var query = _context.AttendanceRecords
                .Include(a => a.Employee).ThenInclude(e => e!.Department)
                .AsQueryable();

            // Employees only see their own records
            if (GetCurrentRole() == "Employee")
            {
                int empId = int.Parse(User.FindFirst("EmployeeID")?.Value ?? "0");
                query = query.Where(a => a.EmployeeID == empId);
            }
            else if (employeeId.HasValue)
            {
                query = query.Where(a => a.EmployeeID == employeeId);
            }

            query = query.Where(a => a.AttendanceDate >= dateFrom && a.AttendanceDate <= dateTo);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.AttendanceStatus == status);

            var records = await query
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.Employee!.LastName)
                .ToListAsync();

            var employees = await _context.Employees
                .Where(e => e.Status == "Active")
                .OrderBy(e => e.LastName)
                .ToListAsync();

            var ruleSummary = _computeService.GetRuleSummary();

            ViewBag.Employees   = employees;
            ViewBag.DateFrom    = dateFrom.Value.ToString("yyyy-MM-dd");
            ViewBag.DateTo      = dateTo.Value.ToString("yyyy-MM-dd");
            ViewBag.EmployeeId  = employeeId;
            ViewBag.Status      = status;
            ViewBag.RuleSummary = ruleSummary;

            return View(records);
        }

        // ── Time In ──────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimeIn(int employeeId)
        {
            var today = DateTime.Today;
            var now   = DateTime.Now.TimeOfDay;

            // Check if already timed in today
            var existing = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.EmployeeID == employeeId && a.AttendanceDate == today);

            if (existing != null && existing.TimeIn.HasValue)
            {
                TempData["Error"] = "Already timed in today.";
                return RedirectToAction(nameof(Index));
            }

            var record = existing ?? new AttendanceRecord
            {
                EmployeeID      = employeeId,
                AttendanceDate  = today,
                IsManualEntry   = false
            };

            record.TimeIn = now;
            _computeService.Compute(record);

            if (existing == null)
                _context.AttendanceRecords.Add(record);

            await _context.SaveChangesAsync();

            string lateMsg = record.IsLate
                ? $" (Late by {record.LateMinutes:F0} minute(s))"
                : string.Empty;

            TempData["Success"] = $"Time In recorded at {now:hh\\:mm\\:ss tt}{lateMsg}.";

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "TimeIn", "Attendance",
                $"Employee ID {employeeId} timed in at {now:hh\\:mm\\:ss}{lateMsg}",
                GetClientIP());

            return RedirectToAction(nameof(Index));
        }

        // ── Time Out ─────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimeOut(int employeeId)
        {
            var today = DateTime.Today;
            var now   = DateTime.Now.TimeOfDay;

            var record = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.EmployeeID == employeeId && a.AttendanceDate == today);

            if (record == null || !record.TimeIn.HasValue)
            {
                TempData["Error"] = "No time-in record found for today.";
                return RedirectToAction(nameof(Index));
            }

            if (record.TimeOut.HasValue)
            {
                TempData["Error"] = "Already timed out today.";
                return RedirectToAction(nameof(Index));
            }

            record.TimeOut = now;
            _computeService.Compute(record);

            await _context.SaveChangesAsync();

            string overtimeMsg = record.HasOvertime
                ? $" | Overtime: {record.OvertimeMinutes:F0} min"
                : string.Empty;

            TempData["Success"] = $"Time Out recorded at {now:hh\\:mm\\:ss tt}. " +
                                  $"Total hours: {record.TotalHoursWorked:F2}{overtimeMsg}.";

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "TimeOut", "Attendance",
                $"Employee ID {employeeId} timed out at {now:hh\\:mm\\:ss}",
                GetClientIP());

            return RedirectToAction(nameof(Index));
        }

        // ── Manual Adjustment (HR Admin / HR Staff only) ─────────────────────────

        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Adjust(int id)
        {
            var record = await _context.AttendanceRecords
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.AttendanceID == id);

            if (record == null) return NotFound();

            var ruleSummary = _computeService.GetRuleSummary();
            ViewBag.RuleSummary = ruleSummary;

            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Adjust(AttendanceRecord model, string? remarks)
        {
            var record = await _context.AttendanceRecords.FindAsync(model.AttendanceID);
            if (record == null) return NotFound();

            string oldValues = $"TimeIn:{record.TimeIn}, TimeOut:{record.TimeOut}, Status:{record.AttendanceStatus}";

            record.TimeIn           = model.TimeIn;
            record.TimeOut          = model.TimeOut;
            record.AttendanceStatus = model.AttendanceStatus;
            record.Remarks          = remarks;
            record.IsManualEntry    = true;
            record.AdjustedBy       = GetCurrentUserID();
            record.AdjustedAt       = DateTime.Now;

            // Recompute with updated times
            _computeService.Compute(record);

            await _context.SaveChangesAsync();

            string newValues = $"TimeIn:{record.TimeIn}, TimeOut:{record.TimeOut}, Status:{record.AttendanceStatus}";

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "ManualAdjust", "Attendance",
                $"Adjusted attendance ID {record.AttendanceID} for Employee ID {record.EmployeeID}",
                GetClientIP(), oldValues, newValues);

            TempData["Success"] = "Attendance record adjusted and recomputed successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── Barcode / ID Scanner Endpoint ────────────────────────────────────────
        // Called by a barcode scanner or RFID reader via HTTP POST.
        // The scanner sends the employee's ID number (EmployeeNo or barcode value).
        // Returns JSON so the scanner terminal can display feedback.

        [HttpPost]
        [AllowAnonymous] // Scanner device doesn't have a login session
        public async Task<IActionResult> Scan([FromBody] ScanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.EmployeeNo))
                return Json(new ScanResult { Success = false, Message = "No ID scanned." });

            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeNo == request.EmployeeNo && e.Status == "Active");

            if (employee == null)
                return Json(new ScanResult { Success = false, Message = $"Employee '{request.EmployeeNo}' not found or inactive." });

            var today = DateTime.Today;
            var now   = DateTime.Now.TimeOfDay;

            var existing = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.EmployeeID == employee.EmployeeID && a.AttendanceDate == today);

            string action;
            string message;

            if (existing == null || !existing.TimeIn.HasValue)
            {
                // TIME IN
                var record = existing ?? new AttendanceRecord
                {
                    EmployeeID     = employee.EmployeeID,
                    AttendanceDate = today,
                    IsManualEntry  = false
                };
                record.TimeIn = now;
                _computeService.Compute(record);

                if (existing == null)
                    _context.AttendanceRecords.Add(record);

                await _context.SaveChangesAsync();

                action  = "TIME IN";
                message = record.IsLate
                    ? $"Late by {record.LateMinutes:F0} min"
                    : "On time";

                await _auditService.LogAsync(null, employee.EmployeeNo,
                    "ScanTimeIn", "Attendance",
                    $"{employee.DisplayName} scanned IN at {now:hh\\:mm\\:ss} — {message}",
                    request.DeviceIP);

                return Json(new ScanResult
                {
                    Success      = true,
                    Action       = action,
                    EmployeeNo   = employee.EmployeeNo,
                    EmployeeName = employee.DisplayName,
                    Department   = employee.Department?.DepartmentName ?? "",
                    TimeRecorded = now.ToString(@"hh\:mm\:ss"),
                    Message      = message,
                    IsLate       = record.IsLate
                });
            }
            else if (!existing.TimeOut.HasValue)
            {
                // TIME OUT
                existing.TimeOut = now;
                _computeService.Compute(existing);
                await _context.SaveChangesAsync();

                action  = "TIME OUT";
                message = existing.HasOvertime
                    ? $"OT: {existing.OvertimeMinutes:F0} min | Total: {existing.TotalHoursWorked:F2} hrs"
                    : $"Total: {existing.TotalHoursWorked:F2} hrs";

                await _auditService.LogAsync(null, employee.EmployeeNo,
                    "ScanTimeOut", "Attendance",
                    $"{employee.DisplayName} scanned OUT at {now:hh\\:mm\\:ss} — {message}",
                    request.DeviceIP);

                return Json(new ScanResult
                {
                    Success      = true,
                    Action       = action,
                    EmployeeNo   = employee.EmployeeNo,
                    EmployeeName = employee.DisplayName,
                    Department   = employee.Department?.DepartmentName ?? "",
                    TimeRecorded = now.ToString(@"hh\:mm\:ss"),
                    Message      = message,
                    IsLate       = false
                });
            }
            else
            {
                return Json(new ScanResult
                {
                    Success      = false,
                    EmployeeNo   = employee.EmployeeNo,
                    EmployeeName = employee.DisplayName,
                    Message      = "Already completed attendance for today."
                });
            }
        }

        // ── Scanner Terminal Page (display for scanner kiosk) ─────────────────────
        [AllowAnonymous]
        public IActionResult Scanner()
        {
            return View();
        }

        // ── Summary Report ───────────────────────────────────────────────────────

        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Summary(int? employeeId, int? month, int? year)        {
            month ??= DateTime.Today.Month;
            year  ??= DateTime.Today.Year;

            var startDate = new DateTime(year.Value, month.Value, 1);
            var endDate   = startDate.AddMonths(1).AddDays(-1);

            var query = _context.AttendanceRecords
                .Include(a => a.Employee).ThenInclude(e => e!.Department)
                .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate);

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeID == employeeId);

            var records = await query.OrderBy(a => a.Employee!.LastName)
                                     .ThenBy(a => a.AttendanceDate)
                                     .ToListAsync();

            var summary = records
                .GroupBy(a => a.EmployeeID)
                .Select(g => new AttendanceSummaryRow
                {
                    Employee        = g.First().Employee!,
                    TotalPresent    = g.Count(r => r.AttendanceStatus == "Present"),
                    TotalLate       = g.Count(r => r.AttendanceStatus == "Late"),
                    TotalAbsent     = g.Count(r => r.AttendanceStatus == "Absent"),
                    TotalOnLeave    = g.Count(r => r.AttendanceStatus == "On Leave"),
                    TotalLateMinutes    = g.Sum(r => r.LateMinutes ?? 0),
                    TotalOvertimeMinutes = g.Sum(r => r.OvertimeMinutes ?? 0),
                    TotalHoursWorked    = g.Sum(r => r.TotalHoursWorked ?? 0)
                })
                .ToList();

            var employees = await _context.Employees
                .Where(e => e.Status == "Active").OrderBy(e => e.LastName).ToListAsync();

            ViewBag.Employees  = employees;
            ViewBag.Month      = month;
            ViewBag.Year       = year;
            ViewBag.EmployeeId = employeeId;
            ViewBag.MonthName  = startDate.ToString("MMMM yyyy");

            return View(summary);
        }
    }
}
