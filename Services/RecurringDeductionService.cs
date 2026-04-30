using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;
using PECCI_HRIS.Models;

namespace PECCI_HRIS.Services
{
    public class RecurringDeductionService
    {
        private readonly ApplicationDbContext _context;

        public RecurringDeductionService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generates EmployeeDeduction entries for a given cutoff period
        /// from all active recurring schedules. Skips if already generated.
        /// Returns the number of entries created.
        /// </summary>
        public async Task<int> GenerateForCutoff(int year, int month, string cutoffPeriod)
        {
            // Cutoff start date used to check schedule bounds
            int cutoffDay = cutoffPeriod == "1-15" ? 1 : 16;
            var cutoffDate = new DateTime(year, month, cutoffDay);

            var schedules = await _context.RecurringDeductionSchedules
                .Where(s => s.Status == "Active"
                         && s.StartDate <= cutoffDate
                         && (s.EndDate == null || s.EndDate >= cutoffDate)
                         && (s.TotalInstallments == null || s.AppliedInstallments < s.TotalInstallments)
                         && (s.CutoffPeriod == cutoffPeriod || s.CutoffPeriod == "Both"))
                .ToListAsync();

            int created = 0;
            foreach (var schedule in schedules)
            {
                // Duplicate check: use ScheduleID stored on the deduction description prefix
                // More reliable: check by employee + month + year + cutoff + scheduleID marker
                bool exists = await _context.EmployeeDeductions.AnyAsync(d =>
                    d.EmployeeID   == schedule.EmployeeID &&
                    d.Month        == month &&
                    d.Year         == year &&
                    d.CutoffPeriod == cutoffPeriod &&
                    d.Amount       == schedule.AmountPerCutoff &&
                    d.DeductionType == schedule.DeductionType &&
                    d.Description.StartsWith(schedule.Description + " [Recurring #"));

                if (exists) continue;

                _context.EmployeeDeductions.Add(new EmployeeDeduction
                {
                    EmployeeID    = schedule.EmployeeID,
                    DeductionType = schedule.DeductionType,
                    Description   = $"{schedule.Description} [Recurring #{schedule.AppliedInstallments + 1}]",
                    Amount        = schedule.AmountPerCutoff,
                    CutoffPeriod  = cutoffPeriod,
                    Month         = month,
                    Year          = year,
                    Status        = "Active",
                    CreatedAt     = DateTime.Now
                });

                schedule.AppliedInstallments++;

                // Auto-complete if all installments done
                if (schedule.TotalInstallments.HasValue &&
                    schedule.AppliedInstallments >= schedule.TotalInstallments)
                {
                    schedule.Status = "Completed";
                }

                created++;
            }

            if (created > 0)
                await _context.SaveChangesAsync();

            return created;
        }
    }
}
