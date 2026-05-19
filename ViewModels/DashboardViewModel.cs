namespace PECCI_HRIS.ViewModels
{
    /// <summary>
    /// Main dashboard view model — aggregates all stats, charts, and widgets.
    /// </summary>
    public class DashboardViewModel
    {
        // ── Stat cards ────────────────────────────────────────────────────────
        public int TotalEmployees    { get; set; }
        public int NewHiresThisMonth { get; set; }
        public int PresentToday      { get; set; }
        public int LateToday         { get; set; }
        public int PendingLeaves     { get; set; }
        public int OnLeaveToday      { get; set; }
        public int TotalDepartments  { get; set; }

        // ── Widgets ───────────────────────────────────────────────────────────
        public List<RecentActivityItem> RecentActivities  { get; set; } = new();
        public List<BirthdayItem>       UpcomingBirthdays { get; set; } = new();
        public AttendanceSummaryItem    AttendanceSummary  { get; set; } = new();

        // ── Chart data ────────────────────────────────────────────────────────
        /// <summary>Headcount per department — for bar chart.</summary>
        public List<DepartmentHeadcountItem> DepartmentHeadcounts { get; set; } = new();

        /// <summary>Monthly net payroll cost for the last 6 months — for line chart.</summary>
        public List<MonthlyPayrollItem> MonthlyPayrollCosts { get; set; } = new();

        /// <summary>Leave utilization per leave type (used vs total) — for horizontal bar chart.</summary>
        public List<LeaveUtilizationItem> LeaveUtilization { get; set; } = new();
    }

    public class RecentActivityItem
    {
        public string Username    { get; set; } = string.Empty;
        public string Action      { get; set; } = string.Empty;
        public string Module      { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class BirthdayItem
    {
        public string   EmployeeName { get; set; } = string.Empty;
        public string   Department   { get; set; } = string.Empty;
        public DateTime BirthDate    { get; set; }
    }

    public class AttendanceSummaryItem
    {
        public int Present { get; set; }
        public int Late    { get; set; }
        public int Absent  { get; set; }
        public int OnLeave { get; set; }
        public int Holiday { get; set; }
        public int Total   => Present + Late + Absent + OnLeave + Holiday;
    }

    /// <summary>Active employee count per department for the headcount bar chart.</summary>
    public class DepartmentHeadcountItem
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int    Count          { get; set; }
    }

    /// <summary>Total net payroll released per month (last 6 months) for the trend line chart.</summary>
    public class MonthlyPayrollItem
    {
        public string  MonthLabel { get; set; } = string.Empty; // e.g. "Jan 2026"
        public decimal TotalNet   { get; set; }
    }

    /// <summary>Leave type utilization — used days vs total allocated — for horizontal bar chart.</summary>
    public class LeaveUtilizationItem
    {
        public string  LeaveTypeName { get; set; } = string.Empty;
        public decimal TotalAllocated { get; set; }
        public decimal TotalUsed      { get; set; }
        public decimal UtilizationPct => TotalAllocated > 0
            ? Math.Round(TotalUsed / TotalAllocated * 100, 1) : 0;
    }
}
