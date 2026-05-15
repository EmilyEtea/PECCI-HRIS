namespace PECCI_HRIS.Configuration
{
    /// <summary>
    /// Scanner kiosk configuration — adjustable via appsettings.json.
    /// </summary>
    public class KioskSettings
    {
        public const string SectionName = "KioskSettings";

        /// <summary>Optional API key. If set, scanner must send X-Api-Key header.</summary>
        public string? ApiKey { get; set; }

        /// <summary>Seconds before kiosk resets after a successful scan. Default: 5.</summary>
        public int AutoResetSeconds { get; set; } = 5;

        /// <summary>Seconds before kiosk resets after an error. Default: 4.</summary>
        public int ErrorResetSeconds { get; set; } = 4;

        /// <summary>Debounce delay in ms before auto-submitting scan input. Default: 400.</summary>
        public int ScanTimeoutMs { get; set; } = 400;

        /// <summary>
        /// Seconds a first scan stays "pending" waiting for confirmation.
        /// Default: 10.
        /// </summary>
        public int PendingWindowSeconds { get; set; } = 10;
    }
}
