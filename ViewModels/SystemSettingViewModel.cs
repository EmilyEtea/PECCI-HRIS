using PECCI_HRIS.Models;

namespace PECCI_HRIS.ViewModels
{
    public class SystemSettingViewModel
    {
        public List<SystemSetting> AttendanceSettings { get; set; } = new();
        public List<SystemSetting> PayrollSettings { get; set; } = new();
        public List<SystemSetting> TaxSettings { get; set; } = new();
        public List<SystemSetting> GeneralSettings { get; set; } = new();

        // For the attendance rule preview panel
        public string LateThresholdDisplay { get; set; } = string.Empty;
        public string WorkingHoursDisplay { get; set; } = string.Empty;
    }

    public class SettingUpdateRequest
    {
        public int SettingID { get; set; }
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
    }
}
