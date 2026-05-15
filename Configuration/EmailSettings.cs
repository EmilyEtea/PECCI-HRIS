namespace PECCI_HRIS.Configuration
{
    /// <summary>
    /// SMTP configuration for outbound email notifications.
    /// Values are loaded from appsettings.json → "EmailSettings".
    /// </summary>
    public class EmailSettings
    {
        public const string SectionName = "EmailSettings";

        /// <summary>SMTP server hostname (e.g. smtp.gmail.com).</summary>
        public string SmtpHost { get; set; } = string.Empty;

        /// <summary>SMTP port. 587 for STARTTLS, 465 for SSL.</summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>Use SSL/TLS. Set false for STARTTLS (port 587).</summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>SMTP login username (usually the sender email address).</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>SMTP login password or app password.</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Display name shown in the From field.</summary>
        public string SenderName { get; set; } = "PECCI HRIS";

        /// <summary>From email address.</summary>
        public string SenderEmail { get; set; } = string.Empty;

        /// <summary>
        /// When false, no emails are sent (useful for local dev/testing).
        /// Log messages are written instead.
        /// </summary>
        public bool Enabled { get; set; } = false;
    }
}
