using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;
using PECCI_HRIS.ViewModels;

namespace PECCI_HRIS.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today     = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            var vm = new DashboardViewModel
            {
                TotalEmployees    = await _context.Employees.CountAsync(e => e.Status == "Active"),
                NewHiresThisMonth = await _context.Employees.CountAsync(e => e.DateHired >= thisMonth),
                PresentToday      = await _context.AttendanceRecords.CountAsync(a => a.AttendanceDate == today && a.AttendanceStatus != "Absent"),
                LateToday         = await _context.AttendanceRecords.CountAsync(a => a.AttendanceDate == today && a.AttendanceStatus == "Late"),
                PendingLeaves     = await _context.LeaveApplications.CountAsync(l => l.Status == "Pending"),
                OnLeaveToday      = await _context.LeaveApplications.CountAsync(l => l.Status == "Approved" && l.StartDate <= today && l.EndDate >= today),
                TotalDepartments  = await _context.Departments.CountAsync(d => d.IsActive),
                RecentActivities  = await GetRecentActivities(),
                UpcomingBirthdays = await GetUpcomingBirthdays(),
                AttendanceSummary = await GetAttendanceSummary(today),

                // ── New chart data ────────────────────────────────────────────
                DepartmentHeadcounts = await GetDepartmentHeadcounts(),
                MonthlyPayrollCosts  = await GetMonthlyPayrollCosts(),
                LeaveUtilization     = await GetLeaveUtilization(today.Year)
            };

            return View(vm);
        }

        // ── Existing helpers ──────────────────────────────────────────────────

        private async Task<List<RecentActivityItem>> GetRecentActivities()
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .Select(a => new RecentActivityItem
                {
                    Username    = a.Username ?? "System",
                    Action      = a.Action,
                    Module      = a.Module,
                    Description = a.Description ?? string.Empty,
                    Timestamp   = a.Timestamp
                })
                .ToListAsync();
        }

        private async Task<List<BirthdayItem>> GetUpcomingBirthdays()
        {
            var today     = DateTime.Today;
            var next7Days = today.AddDays(7);

            var employees = await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.Status == "Active")
                .ToListAsync();

            return employees
                .Where(e =>
                {
                    var bday = new DateTime(today.Year, e.DateOfBirth.Month, e.DateOfBirth.Day);
                    if (bday < today) bday = bday.AddYears(1);
                    return bday <= next7Days;
                })
                .Select(e => new BirthdayItem
                {
                    EmployeeName = e.DisplayName,
                    Department   = e.Department?.DepartmentName ?? string.Empty,
                    BirthDate    = e.DateOfBirth
                })
                .OrderBy(b => b.BirthDate.Month).ThenBy(b => b.BirthDate.Day)
                .ToList();
        }

        private async Task<AttendanceSummaryItem> GetAttendanceSummary(DateTime date)
        {
            var records = await _context.AttendanceRecords
                .Where(a => a.AttendanceDate == date)
                .ToListAsync();

            return new AttendanceSummaryItem
            {
                Present = records.Count(r => r.AttendanceStatus == "Present"),
                Late    = records.Count(r => r.AttendanceStatus == "Late"),
                Absent  = records.Count(r => r.AttendanceStatus == "Absent"),
                OnLeave = records.Count(r => r.AttendanceStatus == "On Leave"),
                Holiday = records.Count(r => r.AttendanceStatus == "Holiday")
            };
        }

        // ── New chart helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Returns active employee count grouped by department.
        /// Used for the headcount bar chart on the dashboard.
        /// </summary>
        private async Task<List<DepartmentHeadcountItem>> GetDepartmentHeadcounts()
        {
            return await _context.Employees
                .Where(e => e.Status == "Active")
                .GroupBy(e => e.Department!.DepartmentName)
                .Select(g => new DepartmentHeadcountItem
                {
                    DepartmentName = g.Key ?? "Unknown",
                    Count          = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
        }

        /// <summary>
        /// Returns total net payroll per month for the last 6 months.
        /// Used for the payroll cost trend line chart.
        /// </summary>
        private async Task<List<MonthlyPayrollItem>> GetMonthlyPayrollCosts()
        {
            var sixMonthsAgo = DateTime.Today.AddMonths(-5);
            var start        = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            var records = await _context.PayrollRecords
                .Where(p => p.PeriodStart >= start && p.Status != "Draft")
                .ToListAsync();

            // Group by year+month, sum net pay
            return records
                .GroupBy(p => new { p.PeriodStart.Year, p.PeriodStart.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyPayrollItem
                {
                    MonthLabel = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    TotalNet   = g.Sum(p => p.StoredNetPay)
                })
                .ToList();
        }

        /// <summary>
        /// Returns leave utilization (used vs allocated) per leave type for the current year.
        /// Used for the horizontal bar chart.
        /// </summary>
        private async Task<List<LeaveUtilizationItem>> GetLeaveUtilization(int year)
        {
            var credits = await _context.LeaveCredits
                .Include(lc => lc.LeaveType)
                .Where(lc => lc.Year == year && lc.LeaveType!.IsActive)
                .ToListAsync();

            return credits
                .GroupBy(lc => lc.LeaveType!.LeaveTypeName)
                .Select(g => new LeaveUtilizationItem
                {
                    LeaveTypeName  = g.Key,
                    TotalAllocated = g.Sum(lc => lc.TotalCredits),
                    TotalUsed      = g.Sum(lc => lc.UsedCredits)
                })
                .OrderByDescending(x => x.UtilizationPct)
                .ToList();
        }
    }
}
