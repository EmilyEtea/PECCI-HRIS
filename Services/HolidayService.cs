using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PECCI_HRIS.Configuration;
using PECCI_HRIS.Data;
using PECCI_HRIS.Models;

namespace PECCI_HRIS.Services
{
    /// <summary>
    /// Provides holiday lookup and holiday-pay computation based on the
    /// Philippine Labor Code and DOLE rules.
    ///
    /// Regular holiday (Art. 94):
    ///   - Employee does NOT work → receives 100% of daily rate (paid holiday).
    ///   - Employee WORKS         → receives 200% of daily rate.
    ///
    /// Special non-working holiday:
    ///   - Employee does NOT work → no pay (no-work, no-pay rule).
    ///   - Employee WORKS         → receives 130% of daily rate.
    /// </summary>
    public class HolidayService
    {
        private readonly ApplicationDbContext _context;
        private readonly PayrollSettings _payrollSettings;

        public HolidayService(ApplicationDbContext context,
            IOptions<PayrollSettings> payrollSettings)
        {
            _context = context;
            _payrollSettings = payrollSettings.Value;
        }

        // ── Lookup ────────────────────────────────────────────────────────────

        /// <summary>Returns the holiday on the given date, or null if none.</summary>
        public async Task<Holiday?> GetHolidayAsync(DateTime date)
        {
            var dateOnly = date.Date;
            return await _context.Holidays
                .FirstOrDefaultAsync(h => h.HolidayDate == dateOnly);
        }

        /// <summary>Returns all holidays in the given date range (inclusive).</summary>
        public async Task<List<Holiday>> GetHolidaysInRangeAsync(DateTime from, DateTime to)
        {
            return await _context.Holidays
                .Where(h => h.HolidayDate >= from.Date && h.HolidayDate <= to.Date)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();
        }

        /// <summary>Returns all holidays for a given year.</summary>
        public async Task<List<Holiday>> GetHolidaysByYearAsync(int year)
        {
            return await _context.Holidays
                .Where(h => h.Year == year)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();
        }

        /// <summary>Returns true if the given date is any kind of holiday.</summary>
        public async Task<bool> IsHolidayAsync(DateTime date)
            => await _context.Holidays.AnyAsync(h => h.HolidayDate == date.Date);

        // ── Pay computation ───────────────────────────────────────────────────

        /// <summary>
        /// Computes the holiday pay premium for a single day.
        ///
        /// For a regular holiday where the employee DID NOT work:
        ///   → returns 1 × daily rate (the guaranteed paid-holiday benefit).
        ///
        /// For a regular holiday where the employee DID work:
        ///   → returns (multiplier − 1) × daily rate as the EXTRA premium
        ///      (the base daily rate is already counted in BasicSalary).
        ///
        /// For a special holiday where the employee DID NOT work:
        ///   → returns 0 (no-work, no-pay).
        ///
        /// For a special holiday where the employee DID work:
        ///   → returns (multiplier − 1) × daily rate as the EXTRA premium.
        /// </summary>
        public decimal ComputeHolidayPay(Holiday holiday, decimal monthlySalary, bool employeeWorked)
        {
            decimal dailyRate = monthlySalary / 22m;

            if (holiday.IsRegular)
            {
                if (!employeeWorked)
                    // Paid holiday — employee gets their regular day's pay even without working.
                    // The basic salary already covers normal working days, so we add 1 day here.
                    return Math.Round(dailyRate, 2);

                // Worked on regular holiday → 200% total; premium = 100% extra
                decimal multiplier = _payrollSettings.RegularHolidayRateMultiplier; // 2.00
                return Math.Round(dailyRate * (multiplier - 1m), 2);
            }
            else // Special
            {
                if (!employeeWorked)
                    return 0; // No-work, no-pay

                // Worked on special holiday → 130% total; premium = 30% extra
                decimal multiplier = _payrollSettings.SpecialHolidayRateMultiplier; // 1.30
                return Math.Round(dailyRate * (multiplier - 1m), 2);
            }
        }

        /// <summary>
        /// Computes total holiday pay for a payroll period.
        /// Iterates over all holidays in the period and checks attendance records.
        /// </summary>
        public async Task<(decimal TotalHolidayPay, int HolidayCount)> ComputePeriodHolidayPayAsync(
            int employeeId,
            DateTime periodStart,
            DateTime periodEnd,
            decimal monthlySalary)
        {
            var holidays = await GetHolidaysInRangeAsync(periodStart, periodEnd);
            if (!holidays.Any()) return (0m, 0);

            // Load attendance records for the period once
            var attendance = await _context.AttendanceRecords
                .Where(a => a.EmployeeID == employeeId
                         && a.AttendanceDate >= periodStart
                         && a.AttendanceDate <= periodEnd)
                .ToListAsync();

            decimal total = 0m;
            foreach (var holiday in holidays)
            {
                var record = attendance.FirstOrDefault(a => a.AttendanceDate.Date == holiday.HolidayDate.Date);

                // Employee worked if they have a time-in record and status is not Absent/On Leave
                bool worked = record != null
                    && record.TimeIn.HasValue
                    && record.AttendanceStatus != "Absent"
                    && record.AttendanceStatus != "On Leave";

                total += ComputeHolidayPay(holiday, monthlySalary, worked);
            }

            return (total, holidays.Count);
        }
    }
}
