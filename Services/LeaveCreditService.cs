using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;
using PECCI_HRIS.Models;

namespace PECCI_HRIS.Services
{
    /// <summary>
    /// Handles leave credit allocation and annual refresh.
    /// Leave credits are allocated per year (not per month).
    /// At the start of each new year, credits are refreshed for all active employees.
    /// </summary>
    public class LeaveCreditService
    {
        private readonly ApplicationDbContext _context;

        public LeaveCreditService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Allocates leave credits for a newly hired employee for the current year.
        /// Gender-restricted leave types (e.g. Magna Carta Leave for females,
        /// Paternity Leave for males) are only allocated when the employee's
        /// gender matches.
        /// </summary>
        public async Task AllocateForNewEmployee(int employeeId)
        {
            var leaveTypes = await _context.LeaveTypes.Where(lt => lt.IsActive).ToListAsync();
            int year = DateTime.Today.Year;

            // Load employee gender for restriction check
            var employee = await _context.Employees.FindAsync(employeeId);

            foreach (var lt in leaveTypes)
            {
                // Skip if gender-restricted and employee's gender doesn't match
                if (!string.IsNullOrEmpty(lt.GenderRestriction) &&
                    !string.Equals(lt.GenderRestriction, employee?.Gender, StringComparison.OrdinalIgnoreCase))
                    continue;

                bool exists = await _context.LeaveCredits
                    .AnyAsync(lc => lc.EmployeeID == employeeId &&
                                    lc.LeaveTypeID == lt.LeaveTypeID &&
                                    lc.Year == year);
                if (!exists)
                {
                    _context.LeaveCredits.Add(new LeaveCredit
                    {
                        EmployeeID     = employeeId,
                        LeaveTypeID    = lt.LeaveTypeID,
                        Year           = year,
                        TotalCredits   = lt.DefaultDaysPerYear,
                        UsedCredits    = 0,
                        PendingCredits = 0,
                        CreatedAt      = DateTime.Now
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Annual refresh — run at the start of each new year (Jan 1).
        /// Resets leave credits for all active employees for the new year.
        /// Unused credits from the previous year are NOT carried over by default.
        /// Gender-restricted leave types are only allocated to matching employees.
        /// </summary>
        public async Task<int> RefreshAnnualCredits()
        {
            int year = DateTime.Today.Year;
            var leaveTypes = await _context.LeaveTypes.Where(lt => lt.IsActive).ToListAsync();
            var activeEmployees = await _context.Employees
                .Where(e => e.Status == "Active").ToListAsync();

            int count = 0;
            foreach (var emp in activeEmployees)
            {
                foreach (var lt in leaveTypes)
                {
                    // Skip gender-restricted types that don't match
                    if (!string.IsNullOrEmpty(lt.GenderRestriction) &&
                        !string.Equals(lt.GenderRestriction, emp.Gender, StringComparison.OrdinalIgnoreCase))
                        continue;

                    bool exists = await _context.LeaveCredits
                        .AnyAsync(lc => lc.EmployeeID == emp.EmployeeID &&
                                        lc.LeaveTypeID == lt.LeaveTypeID &&
                                        lc.Year == year);
                    if (!exists)
                    {
                        _context.LeaveCredits.Add(new LeaveCredit
                        {
                            EmployeeID     = emp.EmployeeID,
                            LeaveTypeID    = lt.LeaveTypeID,
                            Year           = year,
                            TotalCredits   = lt.DefaultDaysPerYear,
                            UsedCredits    = 0,
                            PendingCredits = 0,
                            CreatedAt      = DateTime.Now
                        });
                        count++;
                    }
                }
            }

            if (count > 0)
                await _context.SaveChangesAsync();

            return count;
        }

        /// <summary>
        /// Manually trigger annual refresh from the admin panel.
        /// Gender-restricted leave types are only allocated to matching employees.
        /// </summary>
        public async Task<int> ManualRefresh(int year)
        {
            var leaveTypes = await _context.LeaveTypes.Where(lt => lt.IsActive).ToListAsync();
            var activeEmployees = await _context.Employees
                .Where(e => e.Status == "Active").ToListAsync();

            int count = 0;
            foreach (var emp in activeEmployees)
            {
                foreach (var lt in leaveTypes)
                {
                    // Skip gender-restricted types that don't match
                    if (!string.IsNullOrEmpty(lt.GenderRestriction) &&
                        !string.Equals(lt.GenderRestriction, emp.Gender, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var existing = await _context.LeaveCredits
                        .FirstOrDefaultAsync(lc => lc.EmployeeID == emp.EmployeeID &&
                                                   lc.LeaveTypeID == lt.LeaveTypeID &&
                                                   lc.Year == year);
                    if (existing == null)
                    {
                        _context.LeaveCredits.Add(new LeaveCredit
                        {
                            EmployeeID     = emp.EmployeeID,
                            LeaveTypeID    = lt.LeaveTypeID,
                            Year           = year,
                            TotalCredits   = lt.DefaultDaysPerYear,
                            UsedCredits    = 0,
                            PendingCredits = 0,
                            CreatedAt      = DateTime.Now
                        });
                        count++;
                    }
                    else
                    {
                        // Reset used/pending, keep total
                        existing.UsedCredits    = 0;
                        existing.PendingCredits = 0;
                        existing.TotalCredits   = lt.DefaultDaysPerYear;
                        existing.UpdatedAt      = DateTime.Now;
                        count++;
                    }
                }
            }

            if (count > 0)
                await _context.SaveChangesAsync();

            return count;
        }
    }
}
