namespace PECCI_HRIS.Configuration
{
    /// <summary>
    /// Attendance rules — all values are adjustable via appsettings.json
    /// or the Settings page in the admin panel.
    /// </summary>
    public class AttendanceSettings
    {
        public const string SectionName = "AttendanceSettings";

        /// <summary>Official work start time (HH:mm).</summary>
        public string WorkStartTime { get; set; } = "08:00";

        /// <summary>Official work end time (HH:mm).</summary>
        public string WorkEndTime { get; set; } = "17:00";

        /// <summary>
        /// Grace period in MINUTES after WorkStartTime.
        /// Default: 5 → employees can time-in up to 08:05 without being marked late.
        /// 08:06 onwards = late.
        /// </summary>
        public int GracePeriodMinutes { get; set; } = 5;

        /// <summary>Additional grace period in SECONDS (fine-grained control).</summary>
        public int GracePeriodSeconds { get; set; } = 0;

        /// <summary>Minimum overtime minutes before overtime pay is credited.</summary>
        public int OvertimeThresholdMinutes { get; set; } = 30;

        /// <summary>Lunch break start (HH:mm).</summary>
        public string LunchBreakStartTime { get; set; } = "12:00";

        /// <summary>Lunch break end (HH:mm).</summary>
        public string LunchBreakEndTime { get; set; } = "13:00";

        /// <summary>Lunch break duration in minutes (used in hours-worked computation).</summary>
        public int LunchBreakDurationMinutes { get; set; } = 60;

        /// <summary>How many minutes before WorkStartTime employees are allowed to time-in.</summary>
        public int AllowTimeInBeforeMinutes { get; set; } = 30;

        // ── Deduction types ──────────────────────────────────────────────────────

        /// <summary>
        /// How absent deductions are computed.
        /// Options: "PerDay" | "PerHour"
        /// </summary>
        public string AbsentDeductionType { get; set; } = "PerDay";

        /// <summary>
        /// How late deductions are computed.
        /// Options: "PerMinute" | "PerHour" | "FixedAmount" | "None"
        /// </summary>
        public string LateDeductionType { get; set; } = "PerMinute";

        /// <summary>
        /// Fixed deduction amount per late minute (0 = auto-compute from daily rate).
        /// Set to 0 to let the system derive it from the employee's daily rate.
        /// </summary>
        public decimal LateDeductionAmountPerMinute { get; set; } = 0;

        /// <summary>
        /// How undertime deductions are computed.
        /// Options: "PerMinute" | "PerHour" | "FixedAmount" | "None"
        /// </summary>
        public string UndertimeDeductionType { get; set; } = "PerMinute";

        /// <summary>
        /// Fixed deduction amount per undertime minute (0 = auto-compute from daily rate).
        /// </summary>
        public decimal UndertimeDeductionAmountPerMinute { get; set; } = 0;

        // ── Computed helpers ─────────────────────────────────────────────────────

        public TimeSpan WorkStart => TimeSpan.Parse(WorkStartTime);
        public TimeSpan WorkEnd   => TimeSpan.Parse(WorkEndTime);
        public TimeSpan LunchStart => TimeSpan.Parse(LunchBreakStartTime);
        public TimeSpan LunchEnd   => TimeSpan.Parse(LunchBreakEndTime);

        /// <summary>
        /// The exact cutoff after which a time-in is considered LATE.
        /// e.g. 08:00 + 5 min + 0 sec = 08:05:00 → 08:05:01 is late.
        /// </summary>
        public TimeSpan LateThreshold =>
            WorkStart.Add(TimeSpan.FromMinutes(GracePeriodMinutes))
                     .Add(TimeSpan.FromSeconds(GracePeriodSeconds));

        /// <summary>Total working hours per day (excluding lunch).</summary>
        public double WorkingHoursPerDay =>
            (WorkEnd - WorkStart).TotalHours - (LunchBreakDurationMinutes / 60.0);

        /// <summary>Working minutes per day.</summary>
        public double WorkingMinutesPerDay => WorkingHoursPerDay * 60;
    }
}
