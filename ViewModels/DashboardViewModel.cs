namespace PECCI_HRIS.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int NewHiresThisMonth { get; set; }
        public int PresentToday { get; set; }
        public int LateToday { get; set; }
        public int PendingLeaves { get; set; }
        public int OnLeaveToday { get; set; }
        public int TotalDepartments { get; set; }
        public List<RecentActivityItem> RecentActivities { get; set; } = new();
        public List<BirthdayItem> UpcomingBirthdays { get; set; } = new();
        public AttendanceSummaryItem AttendanceSummary { get; set; } = new();
    }

    public class RecentActivityItem
    {
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class BirthdayItem
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
    }

    public class AttendanceSummaryItem
    {
        public int Present { get; set; }
        public int Late { get; set; }
        public int Absent { get; set; }
        public int OnLeave { get; set; }
        public int Holiday { get; set; }
        public int Total => Present + Late + Absent + OnLeave + Holiday;
    }
}
