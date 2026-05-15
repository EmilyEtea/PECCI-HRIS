using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PECCI_HRIS.Configuration;
using PECCI_HRIS.Data;

namespace PECCI_HRIS.Services
{
    /// <summary>
    /// Reads settings from the SystemSettings table and overlays them on top of
    /// the values loaded from appsettings.json.
    ///
    /// This means:
    ///   - appsettings.json provides the defaults / fallback.
    ///   - Any key present in SystemSettings overrides the appsettings.json value.
    ///   - Changes made via the Settings UI take effect immediately on the next
    ///     request (values are re-read from DB each call; add caching if needed).
    /// </summary>
    public class SystemSettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly AttendanceSettings _attendanceDefaults;
        private readonly PayrollSettings _payrollDefaults;

        public SystemSettingsService(
            ApplicationDbContext context,
            IOptions<AttendanceSettings> attendanceDefaults,
            IOptions<PayrollSettings> payrollDefaults)
        {
            _context = context;
            _attendanceDefaults = attendanceDefaults.Value;
            _payrollDefaults    = payrollDefaults.Value;
        }

        // ── Public accessors ──────────────────────────────────────────────────

        /// <summary>
        /// Returns an AttendanceSettings instance with DB overrides applied.
        /// </summary>
        public async Task<AttendanceSettings> GetAttendanceSettingsAsync()
        {
            var dbSettings = await GetGroupAsync("Attendance");

            var s = Clone(_attendanceDefaults);
            s.WorkStartTime                  = Get(dbSettings, "WorkStartTime",                  s.WorkStartTime);
            s.WorkEndTime                    = Get(dbSettings, "WorkEndTime",                    s.WorkEndTime);
            s.GracePeriodMinutes             = GetInt(dbSettings, "GracePeriodMinutes",          s.GracePeriodMinutes);
            s.GracePeriodSeconds             = GetInt(dbSettings, "GracePeriodSeconds",          s.GracePeriodSeconds);
            s.OvertimeThresholdMinutes       = GetInt(dbSettings, "OvertimeThresholdMinutes",    s.OvertimeThresholdMinutes);
            s.LunchBreakStartTime            = Get(dbSettings, "LunchBreakStartTime",            s.LunchBreakStartTime);
            s.LunchBreakEndTime              = Get(dbSettings, "LunchBreakEndTime",              s.LunchBreakEndTime);
            s.LunchBreakDurationMinutes      = GetInt(dbSettings, "LunchBreakDurationMinutes",   s.LunchBreakDurationMinutes);
            s.LateDeductionType              = Get(dbSettings, "LateDeductionType",              s.LateDeductionType);
            s.LateDeductionAmountPerMinute   = GetDecimal(dbSettings, "LateDeductionAmountPerMinute", s.LateDeductionAmountPerMinute);
            s.UndertimeDeductionType         = Get(dbSettings, "UndertimeDeductionType",         s.UndertimeDeductionType);
            s.UndertimeDeductionAmountPerMinute = GetDecimal(dbSettings, "UndertimeDeductionAmountPerMinute", s.UndertimeDeductionAmountPerMinute);
            return s;
        }

        /// <summary>
        /// Returns a PayrollSettings instance with DB overrides applied.
        /// </summary>
        public async Task<PayrollSettings> GetPayrollSettingsAsync()
        {
            // Payroll rates are split across "Payroll" and "Tax" groups in SystemSettings
            var payrollDb = await GetGroupAsync("Payroll");
            var taxDb     = await GetGroupAsync("Tax");
            // Merge both into one dictionary (Tax keys take precedence for tax fields)
            var dbSettings = payrollDb
                .Concat(taxDb)
                .GroupBy(kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.Last().Value);

            var s = Clone(_payrollDefaults);
            s.OvertimeRateMultiplier         = GetDecimal(dbSettings, "OvertimeRateMultiplier",         s.OvertimeRateMultiplier);
            s.RestDayOvertimeRateMultiplier  = GetDecimal(dbSettings, "RestDayOvertimeRateMultiplier",  s.RestDayOvertimeRateMultiplier);
            s.SpecialHolidayRateMultiplier   = GetDecimal(dbSettings, "SpecialHolidayRateMultiplier",   s.SpecialHolidayRateMultiplier);
            s.RegularHolidayRateMultiplier   = GetDecimal(dbSettings, "RegularHolidayRateMultiplier",   s.RegularHolidayRateMultiplier);
            s.NightDifferentialRate          = GetDecimal(dbSettings, "NightDifferentialRate",          s.NightDifferentialRate);
            s.NightDifferentialStartTime     = Get(dbSettings, "NightDifferentialStartTime",            s.NightDifferentialStartTime);
            s.NightDifferentialEndTime       = Get(dbSettings, "NightDifferentialEndTime",              s.NightDifferentialEndTime);
            s.SSSEmployeeRate                = GetDecimal(dbSettings, "SSSEmployeeRate",                s.SSSEmployeeRate);
            s.PhilHealthRate                 = GetDecimal(dbSettings, "PhilHealthRate",                 s.PhilHealthRate);
            s.PagIbigRate                    = GetDecimal(dbSettings, "PagIbigRate",                    s.PagIbigRate);
            s.PagIbigMaxContribution         = GetDecimal(dbSettings, "PagIbigMaxContribution",         s.PagIbigMaxContribution);
            s.TaxTableType                   = Get(dbSettings, "TaxTableType",                          s.TaxTableType);
            return s;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task<Dictionary<string, string>> GetGroupAsync(string group)
        {
            return await _context.SystemSettings
                .Where(s => s.SettingGroup == group)
                .ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
        }

        private static string Get(Dictionary<string, string> d, string key, string fallback)
            => d.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : fallback;

        private static int GetInt(Dictionary<string, string> d, string key, int fallback)
            => d.TryGetValue(key, out var v) && int.TryParse(v, out var r) ? r : fallback;

        private static decimal GetDecimal(Dictionary<string, string> d, string key, decimal fallback)
            => d.TryGetValue(key, out var v) && decimal.TryParse(v, out var r) ? r : fallback;

        private static AttendanceSettings Clone(AttendanceSettings s) => new()
        {
            WorkStartTime                    = s.WorkStartTime,
            WorkEndTime                      = s.WorkEndTime,
            GracePeriodMinutes               = s.GracePeriodMinutes,
            GracePeriodSeconds               = s.GracePeriodSeconds,
            OvertimeThresholdMinutes         = s.OvertimeThresholdMinutes,
            LunchBreakStartTime              = s.LunchBreakStartTime,
            LunchBreakEndTime                = s.LunchBreakEndTime,
            LunchBreakDurationMinutes        = s.LunchBreakDurationMinutes,
            AllowTimeInBeforeMinutes         = s.AllowTimeInBeforeMinutes,
            AbsentDeductionType              = s.AbsentDeductionType,
            LateDeductionType                = s.LateDeductionType,
            LateDeductionAmountPerMinute     = s.LateDeductionAmountPerMinute,
            UndertimeDeductionType           = s.UndertimeDeductionType,
            UndertimeDeductionAmountPerMinute = s.UndertimeDeductionAmountPerMinute,
        };

        private static PayrollSettings Clone(PayrollSettings s) => new()
        {
            CutoffDay1                       = s.CutoffDay1,
            CutoffDay2                       = s.CutoffDay2,
            OvertimeRateMultiplier           = s.OvertimeRateMultiplier,
            RestDayOvertimeRateMultiplier    = s.RestDayOvertimeRateMultiplier,
            SpecialHolidayRateMultiplier     = s.SpecialHolidayRateMultiplier,
            RegularHolidayRateMultiplier     = s.RegularHolidayRateMultiplier,
            NightDifferentialRate            = s.NightDifferentialRate,
            NightDifferentialStartTime       = s.NightDifferentialStartTime,
            NightDifferentialEndTime         = s.NightDifferentialEndTime,
            SSSEmployeeRate                  = s.SSSEmployeeRate,
            PhilHealthRate                   = s.PhilHealthRate,
            PagIbigRate                      = s.PagIbigRate,
            PagIbigMaxContribution           = s.PagIbigMaxContribution,
            TaxTableType                     = s.TaxTableType,
        };
    }
}
