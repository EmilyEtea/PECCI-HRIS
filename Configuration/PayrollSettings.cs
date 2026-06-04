namespace PECCI_HRIS.Configuration
{
    /// <summary>
    /// Payroll computation rules — all values are adjustable via appsettings.json
    /// or the Settings page in the admin panel.
    /// Based on Philippine labor law and BIR TRAIN Law (RA 10963) as amended.
    /// </summary>
    public class PayrollSettings
    {
        public const string SectionName = "PayrollSettings";

        /// <summary>First cutoff day of the month (default: 15th).</summary>
        public int CutoffDay1 { get; set; } = 15;

        /// <summary>Second cutoff day of the month (default: last day / 30th).</summary>
        public int CutoffDay2 { get; set; } = 30;

        // ── Overtime rates (PECCI HR Policy Manual, Part II §6) ─────────────────
        //
        // PECCI uses three rate categories rather than the DOLE minimum 1.25×:
        //
        //  Completion Rate   — employee completed the required 8-hour workday
        //    Daytime  = 1.50 (150%)
        //    Nighttime = 2.00 (200%)
        //
        //  No-Completion Rate — employee did NOT complete 8 hours before OT
        //    Daytime  = 1.30 (130%)
        //    Nighttime = 1.80 (180%)
        //
        //  Holiday Rate — OT rendered on a regular or special holiday
        //    Daytime  = 1.00 (100% additional on top of holiday pay)
        //    Nighttime = 1.50 (150% additional)
        //
        // "OvertimeRateMultiplier" is retained as the fallback/default and maps to
        // Completion-Daytime so existing code paths still compile unchanged.

        /// <summary>
        /// Completion-Rate daytime OT multiplier (PECCI §6.3.9.1a).
        /// Employee completed the 8-hour workday. Default: 1.50 (150%).
        /// </summary>
        public decimal OvertimeRateMultiplier { get; set; } = 1.50m;

        /// <summary>
        /// Completion-Rate nighttime OT multiplier (PECCI §6.3.9.1b).
        /// Employee completed the 8-hour workday, OT falls in night window. Default: 2.00 (200%).
        /// </summary>
        public decimal OvertimeCompletionNightMultiplier { get; set; } = 2.00m;

        /// <summary>
        /// No-Completion-Rate daytime OT multiplier (PECCI §6.3.9.2a).
        /// Employee did NOT complete 8 hours before OT. Default: 1.30 (130%).
        /// </summary>
        public decimal OvertimeNoCompletionDayMultiplier { get; set; } = 1.30m;

        /// <summary>
        /// No-Completion-Rate nighttime OT multiplier (PECCI §6.3.9.2b).
        /// Employee did NOT complete 8 hours before OT, night window. Default: 1.80 (180%).
        /// </summary>
        public decimal OvertimeNoCompletionNightMultiplier { get; set; } = 1.80m;

        /// <summary>
        /// Holiday-Rate daytime OT multiplier (PECCI §6.3.9.3a).
        /// OT rendered on a holiday. Default: 1.00 (100% of hourly rate added).
        /// </summary>
        public decimal OvertimeHolidayDayMultiplier { get; set; } = 1.00m;

        /// <summary>
        /// Holiday-Rate nighttime OT multiplier (PECCI §6.3.9.3b).
        /// OT rendered on a holiday, night window. Default: 1.50 (150%).
        /// </summary>
        public decimal OvertimeHolidayNightMultiplier { get; set; } = 1.50m;

        /// <summary>
        /// Rest day overtime multiplier (kept for backward compatibility).
        /// Maps to No-Completion daytime rate. Default: 1.30 (130%).
        /// </summary>
        public decimal RestDayOvertimeRateMultiplier { get; set; } = 1.30m;

        /// <summary>Special non-working holiday base rate multiplier (default: 1.30).</summary>
        public decimal SpecialHolidayRateMultiplier { get; set; } = 1.30m;

        /// <summary>Regular holiday base rate multiplier (default: 2.00 = 200%).</summary>
        public decimal RegularHolidayRateMultiplier { get; set; } = 2.00m;

        /// <summary>
        /// Night differential additional rate (PECCI §7.3.1).
        /// Additional 50% of hourly rate for nighttime work. Default: 0.50.
        /// </summary>
        public decimal NightDifferentialRate { get; set; } = 0.50m;

        /// <summary>Night differential start time (HH:mm). Default: 18:00 per PECCI §7.</summary>
        public string NightDifferentialStartTime { get; set; } = "18:00";

        /// <summary>Night differential end time (HH:mm). Default: 06:00.</summary>
        public string NightDifferentialEndTime { get; set; } = "06:00";

        /// <summary>
        /// Required hours per workday before OT qualifies as Completion-Rate.
        /// Default: 8.0 hours. Employees who have not completed this are on No-Completion rate.
        /// </summary>
        public double RequiredHoursPerDay { get; set; } = 8.0;

        // ── Government mandatory contributions ───────────────────────────────────

        /// <summary>SSS employee share rate (default: 4.5%).</summary>
        public decimal SSSEmployeeRate { get; set; } = 0.045m;

        /// <summary>PhilHealth employee share rate (default: 2.5% of basic salary).</summary>
        public decimal PhilHealthRate { get; set; } = 0.025m;

        /// <summary>Pag-IBIG employee contribution rate (default: 2%).</summary>
        public decimal PagIbigRate { get; set; } = 0.02m;

        /// <summary>Pag-IBIG maximum monthly employee contribution (default: ₱100).</summary>
        public decimal PagIbigMaxContribution { get; set; } = 100.00m;

        /// <summary>
        /// BIR tax table to use.
        /// Options: "BIR_TRAIN_LAW_2023" | "BIR_TRAIN_LAW_2018" | "CUSTOM"
        /// </summary>
        public string TaxTableType { get; set; } = "BIR_TRAIN_LAW_2023";
    }
}
