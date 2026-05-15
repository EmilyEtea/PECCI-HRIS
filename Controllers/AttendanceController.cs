using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PECCI_HRIS.Configuration;
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
        private readonly KioskSettings _kioskSettings;

        public AttendanceController(ApplicationDbContext context,
            AttendanceComputationService computeService,
            AuditService auditService,
            IOptions<KioskSettings> kioskSettings)
        {
            _context = context;
            _computeService = computeService;
            _auditService = auditService;
            _kioskSettings = kioskSettings.Value;
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
        // Supports:
        //   - Lookup by EmployeeNo (exact match)
        //   - Lookup by employee name (barcode contains name from ID card)
        //
        // Double-scan confirmation logic:
        //   - 1st scan → "PENDING TIME IN" (confirm required)
        //   - 2nd scan within 10s → confirmed TIME IN
        //   - 3rd scan (or 2nd if already timed in) → "PENDING TIME OUT"
        //   - Next scan within 10s → confirmed TIME OUT
        //
        // Pending state is stored in a static in-memory dictionary (per-server).
        // For multi-server deployments, replace with IDistributedCache.

        private static readonly Dictionary<int, (string Action, DateTime Expires)> _pendingScans = new();
        private static readonly object _pendingLock = new();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Scan([FromBody] ScanRequest request)
        {
            // ── Optional API key check ────────────────────────────────────────
            // If KioskSettings.ApiKey is set, the scanner terminal must send it
            // in the X-Api-Key header. Requests without it are rejected.
            if (!string.IsNullOrWhiteSpace(_kioskSettings.ApiKey))
            {
                var providedKey = Request.Headers["X-Api-Key"].FirstOrDefault();
                if (providedKey != _kioskSettings.ApiKey)
                    return Json(new ScanResult { Success = false, Message = "Unauthorized." });
            }

            if (string.IsNullOrWhiteSpace(request?.EmployeeNo))
                return Json(new ScanResult { Success = false, Message = "No ID scanned." });

            // ── Employee lookup: try EmployeeNo first, then name ─────────────────
            var scannedValue = request.EmployeeNo.Trim();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeNo == scannedValue && e.Status == "Active");

            // If not found by EmployeeNo, try matching by full name or display name
            // (barcode on ID card may contain the employee's name)
            if (employee == null)
            {
                var allActive = await _context.Employees
                    .Include(e => e.Department)
                    .Where(e => e.Status == "Active")
                    .ToListAsync();

                // Try exact match on DisplayName ("FirstName LastName") or FullName ("LastName, FirstName")
                employee = allActive.FirstOrDefault(e =>
                    string.Equals(e.DisplayName, scannedValue, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(e.FullName, scannedValue, StringComparison.OrdinalIgnoreCase));

                // If still not found, try partial name match (first name OR last name)
                if (employee == null)
                {
                    var nameMatches = allActive.Where(e =>
                        string.Equals(e.FirstName, scannedValue, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(e.LastName, scannedValue, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (nameMatches.Count == 1)
                        employee = nameMatches[0];
                    else if (nameMatches.Count > 1)
                        return Json(new ScanResult
                        {
                            Success = false,
                            Message = $"Multiple employees found with name '{scannedValue}'. Please use Employee No."
                        });
                }
            }

            if (employee == null)
                return Json(new ScanResult
                {
                    Success = false,
                    Message = $"Employee '{scannedValue}' not found or inactive."
                });

            var today = DateTime.Today;
            var now   = DateTime.Now.TimeOfDay;

            var existing = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.EmployeeID == employee.EmployeeID && a.AttendanceDate == today);

            // ── Double-scan confirmation logic ───────────────────────────────────
            // Determine what action is needed next
            string neededAction = (existing == null || !existing.TimeIn.HasValue)
                ? "TIME IN"
                : (!existing.TimeOut.HasValue ? "TIME OUT" : "DONE");

            if (neededAction == "DONE")
                return Json(new ScanResult
                {
                    Success      = false,
                    EmployeeNo   = employee.EmployeeNo,
                    EmployeeName = employee.DisplayName,
                    Message      = "Attendance already completed for today."
                });

            // Check pending state
            bool hasPending = false;
            lock (_pendingLock)
            {
                if (_pendingScans.TryGetValue(employee.EmployeeID, out var pending))
                {
                    if (pending.Action == neededAction && DateTime.Now <= pending.Expires)
                        hasPending = true;
                    else
                        _pendingScans.Remove(employee.EmployeeID); // expired or different action
                }
            }

            if (!hasPending)
            {
                // First scan — set pending, ask to scan again
                lock (_pendingLock)
                {
                    _pendingScans[employee.EmployeeID] = (neededAction, DateTime.Now.AddSeconds(_kioskSettings.PendingWindowSeconds));
                }

                return Json(new ScanResult
                {
                    Success      = true,
                    Action       = $"CONFIRM {neededAction}",
                    EmployeeNo   = employee.EmployeeNo,
                    EmployeeName = employee.DisplayName,
                    Department   = employee.Department?.DepartmentName ?? "",
                    TimeRecorded = now.ToString(@"hh\:mm\:ss"),
                    Message      = $"Scan again within {_kioskSettings.PendingWindowSeconds}s to confirm {neededAction}.",
                    IsLate       = false,
                    IsPending    = true
                });
            }

            // Second scan — confirmed, remove pending and process
            lock (_pendingLock) { _pendingScans.Remove(employee.EmployeeID); }

            string action;
            string message;

            if (neededAction == "TIME IN")
            {
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
            else // TIME OUT
            {
                existing!.TimeOut = now;
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
        }

        // ── Scanner Terminal Page (display for scanner kiosk) ─────────────────────
        [AllowAnonymous]
        public IActionResult Scanner()
        {
            ViewBag.ScanTimeoutMs        = _kioskSettings.ScanTimeoutMs;
            ViewBag.AutoResetSeconds     = _kioskSettings.AutoResetSeconds;
            ViewBag.ErrorResetSeconds    = _kioskSettings.ErrorResetSeconds;
            ViewBag.PendingWindowSeconds = _kioskSettings.PendingWindowSeconds;
            ViewBag.ApiKeyRequired       = !string.IsNullOrWhiteSpace(_kioskSettings.ApiKey);
            ViewBag.ApiKey               = _kioskSettings.ApiKey ?? string.Empty;
            return View();
        }

        // ── Summary Report ───────────────────────────────────────────────────────

        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Summary(int? employeeId, int? month, int? year)
        {
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
            ViewBag.PeriodLabel = startDate.ToString("MMMM yyyy");

            return View(summary);
        }
    }
}
