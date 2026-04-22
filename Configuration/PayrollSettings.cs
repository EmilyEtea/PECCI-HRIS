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

        // ── Overtime rates (DOLE-based) ──────────────────────────────────────────

        /// <summary>Regular overtime multiplier (default: 1.25 = 125%).</summary>
        public decimal OvertimeRateMultiplier { get; set; } = 1.25m;

        /// <summary>Rest day overtime multiplier (default: 1.30 = 130%).</summary>
        public decimal RestDayOvertimeRateMultiplier { get; set; } = 1.30m;

        /// <summary>Special non-working holiday rate multiplier (default: 1.30).</summary>
        public decimal SpecialHolidayRateMultiplier { get; set; } = 1.30m;

        /// <summary>Regular holiday rate multiplier (default: 2.00 = 200%).</summary>
        public decimal RegularHolidayRateMultiplier { get; set; } = 2.00m;

        /// <summary>Night differential rate (default: 0.10 = 10% of hourly rate).</summary>
        public decimal NightDifferentialRate { get; set; } = 0.10m;

        /// <summary>Night differential start time (HH:mm). Default: 22:00.</summary>
        public string NightDifferentialStartTime { get; set; } = "22:00";

        /// <summary>Night differential end time (HH:mm). Default: 06:00.</summary>
        public string NightDifferentialEndTime { get; set; } = "06:00";

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
