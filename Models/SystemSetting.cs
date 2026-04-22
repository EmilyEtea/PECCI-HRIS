using System.ComponentModel.DataAnnotations;

namespace PECCI_HRIS.Models
{
    /// <summary>
    /// Stores adjustable system settings in the database.
    /// These override appsettings.json values when present.
    /// Grouped by SettingGroup for organized display in the admin UI.
    /// </summary>
    public class SystemSetting
    {
        [Key]
        public int SettingID { get; set; }

        [Required, MaxLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        public string SettingValue { get; set; } = string.Empty;

        [MaxLength(50)]
        public string SettingGroup { get; set; } = "General"; // Attendance | Payroll | Tax | General

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string DataType { get; set; } = "string"; // string | int | decimal | bool | time

        [MaxLength(200)]
        public string? AllowedValues { get; set; } // comma-separated for dropdowns

        public bool IsEditable { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int? UpdatedBy { get; set; }
    }
}
