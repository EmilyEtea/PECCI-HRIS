using PECCI_HRIS.Models;

namespace PECCI_HRIS.ViewModels
{
    public class AttendanceSummaryRow
    {
        public Employee Employee { get; set; } = null!;        public int TotalPresent { get; set; }
        public int TotalLate { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalOnLeave { get; set; }
        public double TotalLateMinutes { get; set; }
        public double TotalOvertimeMinutes { get; set; }
        public double TotalHoursWorked { get; set; }

        public string LateMinutesDisplay =>
            TotalLateMinutes >= 60
                ? $"{(int)(TotalLateMinutes / 60)}h {(int)(TotalLateMinutes % 60)}m"
                : $"{(int)TotalLateMinutes}m";

        public string OvertimeDisplay =>
            TotalOvertimeMinutes >= 60
                ? $"{(int)(TotalOvertimeMinutes / 60)}h {(int)(TotalOvertimeMinutes % 60)}m"
                : $"{(int)TotalOvertimeMinutes}m";
    }
}

/// <summary>Request body from barcode/RFID scanner device.</summary>
public class ScanRequest
{
    public string EmployeeNo { get; set; } = string.Empty;
    public string? DeviceIP  { get; set; }
}

/// <summary>Response sent back to the scanner terminal.</summary>
public class ScanResult
{
    public bool   Success      { get; set; }
    public string Action       { get; set; } = string.Empty; // TIME IN | TIME OUT | CONFIRM TIME IN | CONFIRM TIME OUT
    public string EmployeeNo   { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department   { get; set; } = string.Empty;
    public string TimeRecorded { get; set; } = string.Empty;
    public string Message      { get; set; } = string.Empty;
    public bool   IsLate       { get; set; }
    public bool   IsPending    { get; set; } // true = waiting for confirmation scan
}
