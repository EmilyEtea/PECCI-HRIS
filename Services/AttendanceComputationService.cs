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
                // PECCI §6.3.3: An employee who has not completed the required
                // 8-hour workday shall not be allowed to render overtime work.
                // We enforce this by only crediting OT when totalHoursWorked
                // (before OT) reaches the RequiredHoursPerDay threshold.
                if (timeOut > _attendanceSettings.WorkEnd)
                {
                    double regularMinutesWorked = totalMinutes
                        - (timeOut - _attendanceSettings.WorkEnd).TotalMinutes;

                    // Did the employee complete the required work hours?
                    double requiredMinutes = _payrollSettings.RequiredHoursPerDay * 60.0;
                    bool completedRegularDay = regularMinutesWorked >= requiredMinutes;

                    if (completedRegularDay)
                    {
                        double overtimeMinutes = (timeOut - _attendanceSettings.WorkEnd).TotalMinutes;
                        // Only credit overtime if it meets the configured minimum threshold
                        if (overtimeMinutes >= _attendanceSettings.OvertimeThresholdMinutes)
                            record.OvertimeMinutes = Math.Floor(overtimeMinutes);
                        else
                            record.OvertimeMinutes = 0;
                    }
                    else
                    {
                        // Employee did not complete the workday — no OT credit (PECCI §6.3.3)
                        record.OvertimeMinutes = 0;
                    }
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

                // ── Night Differential ────────────────────────────────────────
                record.NightDifferentialMinutes = ComputeNightDifferentialMinutes(record);
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

        // ── Night Differential ────────────────────────────────────────────────

        /// <summary>
        /// Computes the number of minutes an employee worked within the night differential
        /// window (default 22:00 – 06:00) for a single attendance record.
        ///
        /// The window crosses midnight, so it is split into two segments:
        ///   Segment A: NDStart → midnight  (e.g. 22:00 – 24:00)
        ///   Segment B: midnight → NDEnd    (e.g. 00:00 – 06:00)
        ///
        /// We intersect the employee's actual work span with each segment and sum the overlap.
        /// </summary>
        public double ComputeNightDifferentialMinutes(AttendanceRecord record)
        {
            if (!record.TimeIn.HasValue || !record.TimeOut.HasValue) return 0;

            TimeSpan ndStart = TimeSpan.Parse(_payrollSettings.NightDifferentialStartTime); // 22:00
            TimeSpan ndEnd   = TimeSpan.Parse(_payrollSettings.NightDifferentialEndTime);   // 06:00

            TimeSpan timeIn  = record.TimeIn.Value;
            TimeSpan timeOut = record.TimeOut.Value;

            // Handle overnight shifts: if TimeOut < TimeIn, the shift crosses midnight.
            // Normalise by adding 24h to TimeOut so arithmetic works on a linear scale.
            bool overnightShift = timeOut < timeIn;
            if (overnightShift)
                timeOut = timeOut.Add(TimeSpan.FromHours(24));

            double ndMinutes = 0;

            // ── Segment A: NDStart → midnight (22:00 → 24:00) ────────────────
            TimeSpan segAStart = ndStart;                        // 22:00
            TimeSpan segAEnd   = TimeSpan.FromHours(24);         // 24:00 (midnight)

            ndMinutes += OverlapMinutes(timeIn, timeOut, segAStart, segAEnd);

            // ── Segment B: midnight → NDEnd (24:00 → 30:00 in +24h space) ────
            // Shift segment B forward by 24h so it sits on the same linear scale
            // as an overnight timeOut.
            TimeSpan segBStart = TimeSpan.FromHours(24);                    // 00:00 → 24:00
            TimeSpan segBEnd   = TimeSpan.FromHours(24) + ndEnd;            // 06:00 → 30:00

            ndMinutes += OverlapMinutes(timeIn, timeOut, segBStart, segBEnd);

            return Math.Round(ndMinutes, 2);
        }

        /// <summary>Returns the overlapping minutes between [workStart,workEnd) and [segStart,segEnd).</summary>
        private static double OverlapMinutes(TimeSpan workStart, TimeSpan workEnd,
                                             TimeSpan segStart,  TimeSpan segEnd)
        {
            TimeSpan overlapStart = workStart > segStart ? workStart : segStart;
            TimeSpan overlapEnd   = workEnd   < segEnd   ? workEnd   : segEnd;
            double minutes = (overlapEnd - overlapStart).TotalMinutes;
            return minutes > 0 ? minutes : 0;
        }

        /// <summary>
        /// Computes the night differential pay amount for a given number of ND minutes
        /// and the employee's monthly salary.
        ///
        /// PECCI HR Policy Manual §7.3.1:
        ///   An employee shall be paid the night premium of additional 50% of the
        ///   employee's hourly rate for each hour of work from 18:00 to 06:00.
        ///
        /// Formula: ND pay = (NDMinutes / 60) × hourlyRate × NDRate
        /// where NDRate = 0.50 per PECCI policy (configured via NightDifferentialRate).
        /// </summary>
        public decimal ComputeNightDifferentialPay(double ndMinutes, decimal monthlySalary)
        {
            if (ndMinutes <= 0) return 0;

            decimal hourlyRate = (monthlySalary / 22m) / 8m;
            decimal ndHours    = (decimal)ndMinutes / 60m;
            return Math.Round(hourlyRate * ndHours * _payrollSettings.NightDifferentialRate, 2);
        }

        /// <summary>
        /// Computes the night differential pay amount using an explicit rate override.
        /// Used when the rate has been loaded from the SystemSettings DB table.
        /// </summary>
        public decimal ComputeNightDifferentialPay(double ndMinutes, decimal monthlySalary, decimal ndRate)
        {
            if (ndMinutes <= 0) return 0;

            decimal hourlyRate = (monthlySalary / 22m) / 8m;
            decimal ndHours    = (decimal)ndMinutes / 60m;
            return Math.Round(hourlyRate * ndHours * ndRate, 2);
        }

        /// <summary>
        /// Computes total night differential pay for a collection of attendance records.
        /// </summary>
        public decimal ComputeTotalNightDifferentialPay(
            IEnumerable<AttendanceRecord> records, decimal monthlySalary)
        {
            double totalNdMinutes = records.Sum(r => r.NightDifferentialMinutes ?? 0);
            return ComputeNightDifferentialPay(totalNdMinutes, monthlySalary);
        }

        /// <summary>
        /// Computes total night differential pay using an explicit rate override.
        /// </summary>
        public decimal ComputeTotalNightDifferentialPay(
            IEnumerable<AttendanceRecord> records, decimal monthlySalary, decimal ndRate)
        {
            double totalNdMinutes = records.Sum(r => r.NightDifferentialMinutes ?? 0);
            return ComputeNightDifferentialPay(totalNdMinutes, monthlySalary, ndRate);
        }

        /// <summary>
        /// Computes overtime pay for a given number of overtime minutes.
        ///
        /// PECCI HR Policy Manual §6.3.9 defines three rate categories:
        ///   Completion Rate   — employee completed the 8-hour workday
        ///     Daytime:  150%  |  Nighttime: 200%
        ///   No-Completion Rate — employee did NOT complete 8 hours before OT
        ///     Daytime:  130%  |  Nighttime: 180%
        ///   Holiday Rate — OT rendered on a holiday
        ///     Daytime:  100%  |  Nighttime: 150%
        ///
        /// The caller supplies <paramref name="hoursWorkedBeforeOt"/> so this method
        /// can determine Completion vs. No-Completion automatically.
        /// Night-window OT is detected via <paramref name="overtimeStartTime"/>.
        /// </summary>
        /// <param name="overtimeMinutes">Total approved/credited OT minutes.</param>
        /// <param name="monthlySalary">Employee's monthly basic salary.</param>
        /// <param name="hoursWorkedBeforeOt">
        ///   Regular hours worked before OT began.
        ///   Pass the full <see cref="AttendanceRecord.TotalHoursWorked"/> value for the day.
        ///   A value ≥ <see cref="PayrollSettings.RequiredHoursPerDay"/> means Completion Rate.
        /// </param>
        /// <param name="overtimeStartTime">
        ///   The time OT began (usually = official work-end time).
        ///   Used to detect whether the OT window falls inside the night differential period.
        ///   Pass <c>null</c> to always use daytime rate.
        /// </param>
        /// <param name="isHoliday">True when the day is a regular or special holiday.</param>
        public decimal ComputeOvertimePay(
            double overtimeMinutes,
            decimal monthlySalary,
            double hoursWorkedBeforeOt = 8.0,
            TimeSpan? overtimeStartTime = null,
            bool isHoliday = false)
        {
            if (overtimeMinutes <= 0) return 0;

            decimal hourlyRate = (monthlySalary / 22m) / 8m;
            decimal multiplier = SelectOvertimeMultiplier(
                _payrollSettings, hoursWorkedBeforeOt, overtimeStartTime, isHoliday);

            decimal overtimeHours = (decimal)overtimeMinutes / 60m;
            return Math.Round(hourlyRate * multiplier * overtimeHours, 2);
        }

        /// <summary>
        /// Computes overtime pay using explicit rate overrides loaded from the DB settings table.
        /// Signature matches the overload called by PayrollController after loading live settings.
        /// </summary>
        public decimal ComputeOvertimePay(
            double overtimeMinutes,
            decimal monthlySalary,
            PayrollSettings settings,
            double hoursWorkedBeforeOt = 8.0,
            TimeSpan? overtimeStartTime = null,
            bool isHoliday = false)
        {
            if (overtimeMinutes <= 0) return 0;

            decimal hourlyRate = (monthlySalary / 22m) / 8m;
            decimal multiplier = SelectOvertimeMultiplier(
                settings, hoursWorkedBeforeOt, overtimeStartTime, isHoliday);

            decimal overtimeHours = (decimal)overtimeMinutes / 60m;
            return Math.Round(hourlyRate * multiplier * overtimeHours, 2);
        }

        /// <summary>
        /// Selects the correct PECCI overtime multiplier based on:
        ///   1. Whether it is a holiday.
        ///   2. Whether the employee completed the required hours (Completion vs. No-Completion).
        ///   3. Whether the OT start time falls inside the night differential window.
        /// </summary>
        private static decimal SelectOvertimeMultiplier(
            PayrollSettings s,
            double hoursWorkedBeforeOt,
            TimeSpan? overtimeStartTime,
            bool isHoliday)
        {
            bool isNight = IsNightOvertimeStart(s, overtimeStartTime);
            bool isCompletion = hoursWorkedBeforeOt >= s.RequiredHoursPerDay;

            if (isHoliday)
                return isNight ? s.OvertimeHolidayNightMultiplier
                               : s.OvertimeHolidayDayMultiplier;

            if (isCompletion)
                return isNight ? s.OvertimeCompletionNightMultiplier
                               : s.OvertimeRateMultiplier;            // OvertimeRateMultiplier = Completion-Day

            // No-Completion
            return isNight ? s.OvertimeNoCompletionNightMultiplier
                           : s.OvertimeNoCompletionDayMultiplier;
        }

        /// <summary>
        /// Returns true when <paramref name="overtimeStartTime"/> falls inside the configured
        /// night differential window (default 18:00 – 06:00).
        /// </summary>
        private static bool IsNightOvertimeStart(PayrollSettings s, TimeSpan? overtimeStartTime)
        {
            if (overtimeStartTime == null) return false;

            TimeSpan ndStart = TimeSpan.Parse(s.NightDifferentialStartTime); // e.g. 18:00
            TimeSpan ndEnd   = TimeSpan.Parse(s.NightDifferentialEndTime);   // e.g. 06:00
            TimeSpan t       = overtimeStartTime.Value;

            // Night window crosses midnight (ndStart > ndEnd), e.g. 18:00 – 06:00
            if (ndStart > ndEnd)
                return t >= ndStart || t < ndEnd;

            // Night window is within the same day (unlikely for PECCI but handled)
            return t >= ndStart && t < ndEnd;
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
