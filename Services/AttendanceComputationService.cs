using Microsoft.Extensions.Options;
using PECCI_HRIS.Configuration;
using PECCI_HRIS.Models;
namespace PECCI_HRIS.Services
{
    /// <summary>
    /// Handles all attendance computation logic.
    /// Grace period, late minutes, overtime, undertime — all driven by AttendanceSettings.
    /// </summary>
    public class AttendanceComputationService
    {
        private readonly AttendanceSettings _attendanceSettings;
        private readonly PayrollSettings _payrollSettings;

        public AttendanceComputationService(
            IOptions<AttendanceSettings> attendanceSettings,
            IOptions<PayrollSettings> payrollSettings)
        {
            _attendanceSettings = attendanceSettings.Value;
            _payrollSettings = payrollSettings.Value;
        }

        /// <summary>
        /// Computes and fills all computed fields on an AttendanceRecord.
        /// Call this whenever a time-in or time-out is recorded.
        /// </summary>
        public void Compute(AttendanceRecord record)
        {
            if (record.TimeIn == null)
            {
                record.AttendanceStatus = "Absent";
                record.LateMinutes = 0;
                record.OvertimeMinutes = 0;
                record.UndertimeMinutes = 0;
                record.TotalHoursWorked = 0;
                return;
            }

            // ── Late computation ─────────────────────────────────────────────────
            // Grace period: 08:00 + 5 min = 08:05:00
            // 08:05:00 or earlier → NOT late
            // 08:05:01 onwards    → LATE (minutes counted from 08:00, not from 08:05)
            TimeSpan timeIn = record.TimeIn.Value;
            TimeSpan lateThreshold = _attendanceSettings.LateThreshold; // e.g. 08:05:00

            if (timeIn > lateThreshold)
            {
                // Late minutes = actual time-in minus official start (08:00), rounded up
                double rawLateMinutes = (timeIn - _attendanceSettings.WorkStart).TotalMinutes;
                record.LateMinutes = Math.Ceiling(rawLateMinutes);
                record.AttendanceStatus = "Late";
            }
            else
            {
                record.LateMinutes = 0;
            }

            // ── Total hours worked ───────────────────────────────────────────────
            if (record.TimeOut.HasValue)
            {
                TimeSpan timeOut = record.TimeOut.Value;
                double totalMinutes = (timeOut - timeIn).TotalMinutes;

                // Deduct lunch break if employee worked through it
                bool workedThroughLunch = timeIn <= _attendanceSettings.LunchStart && timeOut >= _attendanceSettings.LunchEnd;
                if (workedThroughLunch)
                    totalMinutes -= _attendanceSettings.LunchBreakDurationMinutes;

                // Deduct break time if recorded
                if (record.BreakOut.HasValue && record.BreakIn.HasValue)
                {
                    double breakMinutes = (record.BreakIn.Value - record.BreakOut.Value).TotalMinutes;
                    if (breakMinutes > 0) totalMinutes -= breakMinutes;
                }

                record.TotalHoursWorked = Math.Max(0, totalMinutes / 60.0);

                // ── Overtime ─────────────────────────────────────────────────────
                // Overtime = time worked beyond official end time
                if (timeOut > _attendanceSettings.WorkEnd)
                {
                    double overtimeMinutes = (timeOut - _attendanceSettings.WorkEnd).TotalMinutes;
                    // Only credit overtime if it exceeds the threshold
                    if (overtimeMinutes >= _attendanceSettings.OvertimeThresholdMinutes)
                        record.OvertimeMinutes = Math.Floor(overtimeMinutes);
                    else
                        record.OvertimeMinutes = 0;
                }
                else
                {
                    record.OvertimeMinutes = 0;
                }

                // ── Undertime ────────────────────────────────────────────────────
                // Undertime = left before official end time
                if (timeOut < _attendanceSettings.WorkEnd)
                {
                    record.UndertimeMinutes = Math.Ceiling((_attendanceSettings.WorkEnd - timeOut).TotalMinutes);
                }
                else
                {
                    record.UndertimeMinutes = 0;
                }

                // ── Final status ─────────────────────────────────────────────────
                if (record.AttendanceStatus != "On Leave" && record.AttendanceStatus != "Holiday")
                {
                    if (record.LateMinutes > 0 || record.UndertimeMinutes > 0)
                        record.AttendanceStatus = record.LateMinutes > 0 ? "Late" : "Undertime";
                    else
                        record.AttendanceStatus = "Present";
                }
            }
        }

        /// <summary>
        /// Computes the monetary deduction for late minutes based on the employee's daily rate.
        /// </summary>
        public decimal ComputeLateDeduction(double lateMinutes, decimal monthlySalary)
        {
            if (lateMinutes <= 0) return 0;

            // If a fixed per-minute amount is configured, use it
            if (_attendanceSettings.LateDeductionAmountPerMinute > 0)
                return (decimal)lateMinutes * _attendanceSettings.LateDeductionAmountPerMinute;

            // Otherwise derive from monthly salary
            // Daily rate = monthly / 22 working days
            // Hourly rate = daily / 8 hours
            // Per-minute rate = hourly / 60
            decimal dailyRate = monthlySalary / 22m;
            decimal perMinuteRate = dailyRate / (decimal)_attendanceSettings.WorkingMinutesPerDay;
            return Math.Round((decimal)lateMinutes * perMinuteRate, 2);
        }

        /// <summary>
        /// Computes the monetary deduction for undertime minutes.
        /// </summary>
        public decimal ComputeUndertimeDeduction(double undertimeMinutes, decimal monthlySalary)
        {
            if (undertimeMinutes <= 0) return 0;

            if (_attendanceSettings.UndertimeDeductionAmountPerMinute > 0)
                return (decimal)undertimeMinutes * _attendanceSettings.UndertimeDeductionAmountPerMinute;

            decimal dailyRate = monthlySalary / 22m;
            decimal perMinuteRate = dailyRate / (decimal)_attendanceSettings.WorkingMinutesPerDay;
            return Math.Round((decimal)undertimeMinutes * perMinuteRate, 2);
        }

        /// <summary>
        /// Computes overtime pay for a given number of overtime minutes and hourly rate.
        /// </summary>
        public decimal ComputeOvertimePay(double overtimeMinutes, decimal monthlySalary,
            bool isRestDay = false, bool isSpecialHoliday = false, bool isRegularHoliday = false)
        {
            if (overtimeMinutes <= 0) return 0;

            decimal hourlyRate = (monthlySalary / 22m) / 8m;
            decimal multiplier = _payrollSettings.OvertimeRateMultiplier;

            if (isRegularHoliday)
                multiplier = _payrollSettings.RegularHolidayRateMultiplier * _payrollSettings.OvertimeRateMultiplier;
            else if (isSpecialHoliday)
                multiplier = _payrollSettings.SpecialHolidayRateMultiplier * _payrollSettings.OvertimeRateMultiplier;
            else if (isRestDay)
                multiplier = _payrollSettings.RestDayOvertimeRateMultiplier;

            decimal overtimeHours = (decimal)overtimeMinutes / 60m;
            return Math.Round(hourlyRate * multiplier * overtimeHours, 2);
        }

        /// <summary>
        /// Returns a human-readable summary of the current attendance rules.
        /// </summary>
        public AttendanceRuleSummary GetRuleSummary()
        {
            return new AttendanceRuleSummary
            {
                WorkStart = _attendanceSettings.WorkStartTime,
                WorkEnd = _attendanceSettings.WorkEndTime,
                GracePeriodMinutes = _attendanceSettings.GracePeriodMinutes,
                GracePeriodSeconds = _attendanceSettings.GracePeriodSeconds,
                LateThreshold = _attendanceSettings.LateThreshold.ToString(@"hh\:mm\:ss"),
                OvertimeThresholdMinutes = _attendanceSettings.OvertimeThresholdMinutes,
                LunchBreak = $"{_attendanceSettings.LunchBreakStartTime} – {_attendanceSettings.LunchBreakEndTime}",
                WorkingHoursPerDay = _attendanceSettings.WorkingHoursPerDay,
                LateDeductionType = _attendanceSettings.LateDeductionType,
                UndertimeDeductionType = _attendanceSettings.UndertimeDeductionType
            };
        }
    }

    public class AttendanceRuleSummary
    {
        public string WorkStart { get; set; } = string.Empty;
        public string WorkEnd { get; set; } = string.Empty;
        public int GracePeriodMinutes { get; set; }
        public int GracePeriodSeconds { get; set; }
        public string LateThreshold { get; set; } = string.Empty;
        public int OvertimeThresholdMinutes { get; set; }
        public string LunchBreak { get; set; } = string.Empty;
        public double WorkingHoursPerDay { get; set; }
        public string LateDeductionType { get; set; } = string.Empty;
        public string UndertimeDeductionType { get; set; } = string.Empty;
    }
}
