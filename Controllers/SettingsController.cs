using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;
using PECCI_HRIS.Services;
using PECCI_HRIS.ViewModels;

namespace PECCI_HRIS.Controllers
{
    // HR Staff can view settings but not change them — write actions stay HR Admin only
    [Authorize(Roles = "HR Admin,HR Staff")]
    public class SettingsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly AttendanceComputationService _attendanceService;
        private readonly AuditService _auditService;

        public SettingsController(ApplicationDbContext context,
            AttendanceComputationService attendanceService,
            AuditService auditService)
        {
            _context = context;
            _attendanceService = attendanceService;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var allSettings = await _context.SystemSettings
                .OrderBy(s => s.SettingGroup).ThenBy(s => s.SettingKey)
                .ToListAsync();

            var ruleSummary = _attendanceService.GetRuleSummary();

            var vm = new SystemSettingViewModel
            {
                AttendanceSettings = allSettings.Where(s => s.SettingGroup == "Attendance").ToList(),
                PayrollSettings    = allSettings.Where(s => s.SettingGroup == "Payroll").ToList(),
                TaxSettings        = allSettings.Where(s => s.SettingGroup == "Tax").ToList(),
                GeneralSettings    = allSettings.Where(s => s.SettingGroup == "General").ToList(),
                LateThresholdDisplay = ruleSummary.LateThreshold,
                WorkingHoursDisplay  = $"{ruleSummary.WorkingHoursPerDay:F1} hours/day"
            };

            // HR Staff can view but not edit — pass flag to view
            ViewBag.IsReadOnly = !User.IsInRole("HR Admin");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> UpdateSetting([FromBody] SettingUpdateRequest request)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingID == request.SettingID);

            if (setting == null)
                return Json(new { success = false, message = "Setting not found." });

            if (!setting.IsEditable)
                return Json(new { success = false, message = "This setting cannot be edited." });

            // Validate based on data type
            var (isValid, errorMsg) = ValidateSettingValue(setting.DataType, request.SettingValue, setting.AllowedValues);
            if (!isValid)
                return Json(new { success = false, message = errorMsg });

            string oldValue = setting.SettingValue;
            setting.SettingValue = request.SettingValue.Trim();
            setting.UpdatedAt = DateTime.Now;
            setting.UpdatedBy = GetCurrentUserID();

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                GetCurrentUserID(), GetCurrentUsername(),
                "Update", "Settings",
                $"Changed {setting.SettingKey} from '{oldValue}' to '{setting.SettingValue}'",
                GetClientIP()
            );

            return Json(new { success = true, message = $"{setting.SettingKey} updated successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> BulkUpdate(Dictionary<string, string> settings)
        {
            int updated = 0;
            foreach (var kvp in settings)
            {
                if (!int.TryParse(kvp.Key, out int id)) continue;

                var setting = await _context.SystemSettings.FindAsync(id);
                if (setting == null || !setting.IsEditable) continue;

                var (isValid, _) = ValidateSettingValue(setting.DataType, kvp.Value, setting.AllowedValues);
                if (!isValid) continue;

                setting.SettingValue = kvp.Value.Trim();
                setting.UpdatedAt = DateTime.Now;
                setting.UpdatedBy = GetCurrentUserID();
                updated++;
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                GetCurrentUserID(), GetCurrentUsername(),
                "BulkUpdate", "Settings",
                $"Updated {updated} settings",
                GetClientIP()
            );

            TempData["Success"] = $"{updated} setting(s) saved successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult PreviewAttendanceRule(string workStart, string workEnd,
            int graceMins, int graceSecs, int otThreshold)
        {
            try
            {
                var start = TimeSpan.Parse(workStart);
                var lateThreshold = start
                    .Add(TimeSpan.FromMinutes(graceMins))
                    .Add(TimeSpan.FromSeconds(graceSecs));

                return Json(new
                {
                    success = true,
                    lateThreshold = lateThreshold.ToString(@"hh\:mm\:ss"),
                    description = $"Employees can time-in up to {lateThreshold:hh\\:mm\\:ss}. " +
                                  $"Any time-in after that is marked LATE."
                });
            }
            catch
            {
                return Json(new { success = false, message = "Invalid time format." });
            }
        }

        private static (bool isValid, string error) ValidateSettingValue(
            string dataType, string value, string? allowedValues)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (false, "Value cannot be empty.");

            switch (dataType.ToLower())
            {
                case "int":
                    if (!int.TryParse(value, out _))
                        return (false, "Value must be a whole number.");
                    break;
                case "decimal":
                    if (!decimal.TryParse(value, out _))
                        return (false, "Value must be a number.");
                    break;
                case "bool":
                    if (!bool.TryParse(value, out _))
                        return (false, "Value must be true or false.");
                    break;
                case "time":
                    if (!TimeSpan.TryParse(value, out _))
                        return (false, "Value must be a valid time (HH:mm).");
                    break;
            }

            if (!string.IsNullOrEmpty(allowedValues))
            {
                var allowed = allowedValues.Split(',');
                if (!allowed.Contains(value))
                    return (false, $"Value must be one of: {allowedValues}");
            }

            return (true, string.Empty);
        }
    }
}
