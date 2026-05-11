using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;
using PECCI_HRIS.Models;
using PECCI_HRIS.Services;
using PECCI_HRIS.ViewModels;

namespace PECCI_HRIS.Controllers
{
    [Authorize(Roles = "HR Admin,HR Staff")]
    public class HolidayController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public HolidayController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // ── Index ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(int? year, string? type)
        {
            int selectedYear = year ?? DateTime.Today.Year;

            var query = _context.Holidays.AsQueryable();
            query = query.Where(h => h.Year == selectedYear);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(h => h.HolidayType == type);

            var holidays = await query.OrderBy(h => h.HolidayDate).ToListAsync();

            // Available years: current year ± 2, plus any years already in DB
            var dbYears = await _context.Holidays.Select(h => h.Year).Distinct().ToListAsync();
            var yearRange = Enumerable.Range(DateTime.Today.Year - 1, 4).ToList();
            var availableYears = dbYears.Union(yearRange).OrderBy(y => y).ToList();

            var vm = new HolidayIndexViewModel
            {
                Holidays       = holidays,
                SelectedYear   = selectedYear,
                SelectedType   = type,
                AvailableYears = availableYears
            };

            return View(vm);
        }

        // ── Create ────────────────────────────────────────────────────────────
        public IActionResult Create()
        {
            return View(new HolidayFormViewModel { HolidayDate = DateTime.Today });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HolidayFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Duplicate check
            bool duplicate = await _context.Holidays
                .AnyAsync(h => h.HolidayDate == vm.HolidayDate.Date && h.HolidayType == vm.HolidayType);
            if (duplicate)
            {
                ModelState.AddModelError("", $"A {vm.HolidayType} holiday already exists on {vm.HolidayDate:MMMM d, yyyy}.");
                return View(vm);
            }

            var holiday = new Holiday
            {
                HolidayDate  = vm.HolidayDate.Date,
                HolidayName  = vm.HolidayName.Trim(),
                HolidayType  = vm.HolidayType,
                Year         = vm.HolidayDate.Year,
                IsRecurring  = vm.IsRecurring,
                Remarks      = vm.Remarks?.Trim(),
                CreatedAt    = DateTime.Now,
                CreatedBy    = GetCurrentUserID()
            };

            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Create", "Holiday",
                $"Added {holiday.HolidayType} holiday: {holiday.HolidayName} on {holiday.HolidayDate:yyyy-MM-dd}",
                GetClientIP());

            TempData["Success"] = $"Holiday '{holiday.HolidayName}' added successfully.";
            return RedirectToAction(nameof(Index), new { year = holiday.Year });
        }

        // ── Edit ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday == null) return NotFound();

            var vm = new HolidayFormViewModel
            {
                HolidayID   = holiday.HolidayID,
                HolidayDate = holiday.HolidayDate,
                HolidayName = holiday.HolidayName,
                HolidayType = holiday.HolidayType,
                IsRecurring = holiday.IsRecurring,
                Remarks     = holiday.Remarks
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HolidayFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var holiday = await _context.Holidays.FindAsync(vm.HolidayID);
            if (holiday == null) return NotFound();

            // Duplicate check (exclude self)
            bool duplicate = await _context.Holidays
                .AnyAsync(h => h.HolidayDate == vm.HolidayDate.Date
                            && h.HolidayType == vm.HolidayType
                            && h.HolidayID   != vm.HolidayID);
            if (duplicate)
            {
                ModelState.AddModelError("", $"Another {vm.HolidayType} holiday already exists on {vm.HolidayDate:MMMM d, yyyy}.");
                return View(vm);
            }

            string oldName = holiday.HolidayName;
            holiday.HolidayDate  = vm.HolidayDate.Date;
            holiday.HolidayName  = vm.HolidayName.Trim();
            holiday.HolidayType  = vm.HolidayType;
            holiday.Year         = vm.HolidayDate.Year;
            holiday.IsRecurring  = vm.IsRecurring;
            holiday.Remarks      = vm.Remarks?.Trim();

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Update", "Holiday",
                $"Updated holiday: '{oldName}' → '{holiday.HolidayName}' on {holiday.HolidayDate:yyyy-MM-dd}",
                GetClientIP());

            TempData["Success"] = $"Holiday '{holiday.HolidayName}' updated.";
            return RedirectToAction(nameof(Index), new { year = holiday.Year });
        }

        // ── Delete ────────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday == null) return NotFound();

            int year = holiday.Year;
            string name = holiday.HolidayName;

            _context.Holidays.Remove(holiday);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Delete", "Holiday",
                $"Deleted holiday: {name} on {holiday.HolidayDate:yyyy-MM-dd}",
                GetClientIP());

            TempData["Success"] = $"Holiday '{name}' deleted.";
            return RedirectToAction(nameof(Index), new { year });
        }

        // ── Auto-Populate from Proclamation List ──────────────────────────────
        /// <summary>
        /// Seeds the official Philippine public holidays for the requested year
        /// based on the standard Proclamation list. Skips entries that already exist.
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> AutoPopulate(int year)
        {
            var proclamationHolidays = GetProclamationHolidays(year);

            int added   = 0;
            int skipped = 0;

            foreach (var h in proclamationHolidays)
            {
                bool exists = await _context.Holidays
                    .AnyAsync(x => x.HolidayDate == h.HolidayDate && x.HolidayType == h.HolidayType);

                if (exists) { skipped++; continue; }

                h.CreatedAt = DateTime.Now;
                h.CreatedBy = GetCurrentUserID();
                _context.Holidays.Add(h);
                added++;
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "AutoPopulate", "Holiday",
                $"Auto-populated {added} Philippine holidays for {year} (skipped {skipped} duplicates).",
                GetClientIP());

            if (added > 0)
                TempData["Success"] = $"Added {added} official Philippine holidays for {year}. {(skipped > 0 ? $"{skipped} already existed and were skipped." : "")}";
            else
                TempData["Warning"] = $"All {skipped} holidays for {year} already exist in the calendar.";

            return RedirectToAction(nameof(Index), new { year });
        }

        // ── Proclamation Data ─────────────────────────────────────────────────
        /// <summary>
        /// Returns the official Philippine public holidays for a given year.
        /// Regular holidays follow Republic Act 9492 (fixed dates) and annual
        /// Proclamations for movable dates. Special non-working holidays are
        /// declared by Presidential Proclamation each year.
        ///
        /// Fixed regular holidays (RA 9492):
        ///   Jan 1  – New Year's Day
        ///   Apr 9  – Araw ng Kagitingan (Day of Valor)
        ///   May 1  – Labor Day
        ///   Jun 12 – Independence Day
        ///   Aug 26 – National Heroes Day (last Monday of August — computed)
        ///   Nov 30 – Bonifacio Day
        ///   Dec 25 – Christmas Day
        ///   Dec 30 – Rizal Day
        ///
        /// Movable regular holidays (Proclamation-based):
        ///   Maundy Thursday, Good Friday, Eid'l Fitr, Eid'l Adha
        ///
        /// Fixed special non-working holidays:
        ///   Aug 21 – Ninoy Aquino Day
        ///   Nov 1  – All Saints' Day
        ///   Dec 8  – Feast of the Immaculate Conception
        ///   Dec 31 – New Year's Eve
        ///
        /// Movable special non-working holidays (Proclamation-based):
        ///   Black Saturday, Chinese New Year, EDSA People Power Revolution (Feb 25)
        /// </summary>
        private static List<Holiday> GetProclamationHolidays(int year)
        {
            var list = new List<Holiday>();

            void Add(DateTime date, string name, string type, bool recurring = false, string? remarks = null)
                => list.Add(new Holiday
                {
                    HolidayDate = date.Date,
                    HolidayName = name,
                    HolidayType = type,
                    Year        = year,
                    IsRecurring = recurring,
                    Remarks     = remarks
                });

            // ── Fixed Regular Holidays (RA 9492) ─────────────────────────────
            Add(new DateTime(year, 1,  1),  "New Year's Day",                    "Regular", recurring: true);
            Add(new DateTime(year, 4,  9),  "Araw ng Kagitingan (Day of Valor)", "Regular", recurring: true,
                remarks: "Commemorates the Fall of Bataan");
            Add(new DateTime(year, 5,  1),  "Labor Day",                         "Regular", recurring: true);
            Add(new DateTime(year, 6,  12), "Independence Day",                  "Regular", recurring: true);
            Add(LastMondayOfAugust(year),   "National Heroes Day",               "Regular", recurring: false,
                remarks: "Last Monday of August");
            Add(new DateTime(year, 11, 30), "Bonifacio Day",                     "Regular", recurring: true);
            Add(new DateTime(year, 12, 25), "Christmas Day",                     "Regular", recurring: true);
            Add(new DateTime(year, 12, 30), "Rizal Day",                         "Regular", recurring: true);

            // ── Movable Regular Holidays ──────────────────────────────────────
            // Easter-based: Maundy Thursday & Good Friday
            DateTime easter = ComputeEaster(year);
            Add(easter.AddDays(-3), "Maundy Thursday", "Regular", recurring: false,
                remarks: "3 days before Easter Sunday");
            Add(easter.AddDays(-2), "Good Friday",     "Regular", recurring: false,
                remarks: "2 days before Easter Sunday");

            // Eid'l Fitr & Eid'l Adha — approximate dates based on Islamic calendar.
            // Actual dates are confirmed by Proclamation; these are best estimates.
            var (eidFitr, eidAdha) = GetEidDates(year);
            Add(eidFitr, "Eid'l Fitr (Feast of Ramadan)", "Regular", recurring: false,
                remarks: "Approximate — confirm via official Proclamation");
            Add(eidAdha, "Eid'l Adha (Feast of Sacrifice)", "Regular", recurring: false,
                remarks: "Approximate — confirm via official Proclamation");

            // ── Fixed Special Non-Working Holidays ────────────────────────────
            Add(new DateTime(year, 2,  25), "EDSA People Power Revolution Anniversary", "Special", recurring: true);
            Add(new DateTime(year, 8,  21), "Ninoy Aquino Day",                          "Special", recurring: true);
            Add(new DateTime(year, 11, 1),  "All Saints' Day",                           "Special", recurring: true);
            Add(new DateTime(year, 11, 2),  "All Souls' Day",                            "Special", recurring: false,
                remarks: "Declared by Proclamation annually");
            Add(new DateTime(year, 12, 8),  "Feast of the Immaculate Conception",        "Special", recurring: true);
            Add(new DateTime(year, 12, 31), "New Year's Eve",                            "Special", recurring: true);

            // ── Movable Special Non-Working Holidays ──────────────────────────
            // Black Saturday (day after Good Friday)
            Add(easter.AddDays(-1), "Black Saturday", "Special", recurring: false,
                remarks: "Day before Easter Sunday");

            // Chinese New Year (first day of the Chinese lunisolar calendar)
            Add(GetChineseNewYear(year), "Chinese New Year", "Special", recurring: false,
                remarks: "First day of the Chinese lunisolar calendar");

            return list;
        }

        // ── Date Helpers ──────────────────────────────────────────────────────

        /// <summary>Returns the last Monday of August for the given year.</summary>
        private static DateTime LastMondayOfAugust(int year)
        {
            var lastDay = new DateTime(year, 8, 31);
            int daysBack = ((int)lastDay.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return lastDay.AddDays(-daysBack);
        }

        /// <summary>
        /// Computes Easter Sunday using the Anonymous Gregorian algorithm.
        /// </summary>
        private static DateTime ComputeEaster(int year)
        {
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int month = (h + l - 7 * m + 114) / 31;
            int day   = ((h + l - 7 * m + 114) % 31) + 1;
            return new DateTime(year, month, day);
        }

        /// <summary>
        /// Returns approximate Eid'l Fitr and Eid'l Adha dates for the Philippines.
        /// These are estimates based on the Islamic Hijri calendar conversion.
        /// The official dates are confirmed by Presidential Proclamation.
        /// </summary>
        private static (DateTime EidFitr, DateTime EidAdha) GetEidDates(int year)
        {
            // Approximate Eid dates for known years (Philippines observance)
            var eidFitrMap = new Dictionary<int, DateTime>
            {
                { 2024, new DateTime(2024, 4, 10) },
                { 2025, new DateTime(2025, 3, 31) },
                { 2026, new DateTime(2026, 3, 20) },
                { 2027, new DateTime(2027, 3, 10) },
                { 2028, new DateTime(2028, 2, 27) },
                { 2029, new DateTime(2029, 2, 16) },
                { 2030, new DateTime(2030, 2, 5)  },
            };
            var eidAdhaMap = new Dictionary<int, DateTime>
            {
                { 2024, new DateTime(2024, 6, 17) },
                { 2025, new DateTime(2025, 6, 7)  },
                { 2026, new DateTime(2026, 5, 27) },
                { 2027, new DateTime(2027, 5, 17) },
                { 2028, new DateTime(2028, 5, 5)  },
                { 2029, new DateTime(2029, 4, 24) },
                { 2030, new DateTime(2030, 4, 14) },
            };

            // Fallback: approximate using Islamic calendar cycle (~354.37 days/year)
            // Anchor: Eid'l Fitr 2026 = March 20
            if (!eidFitrMap.TryGetValue(year, out DateTime fitr))
            {
                int delta = year - 2026;
                fitr = new DateTime(2026, 3, 20).AddDays(delta * -10.875);
            }
            if (!eidAdhaMap.TryGetValue(year, out DateTime adha))
            {
                int delta = year - 2026;
                adha = new DateTime(2026, 5, 27).AddDays(delta * -10.875);
            }

            return (fitr.Date, adha.Date);
        }

        /// <summary>
        /// Returns the approximate date of Chinese New Year for the given Gregorian year.
        /// Chinese New Year falls on the second new moon after the winter solstice
        /// (between Jan 21 and Feb 20).
        /// </summary>
        private static DateTime GetChineseNewYear(int year)
        {
            // Known Chinese New Year dates
            var cnyMap = new Dictionary<int, DateTime>
            {
                { 2024, new DateTime(2024, 2, 10) },
                { 2025, new DateTime(2025, 1, 29) },
                { 2026, new DateTime(2026, 2, 17) },
                { 2027, new DateTime(2027, 2, 6)  },
                { 2028, new DateTime(2028, 1, 26) },
                { 2029, new DateTime(2029, 2, 13) },
                { 2030, new DateTime(2030, 2, 3)  },
            };

            if (cnyMap.TryGetValue(year, out DateTime cny)) return cny;

            // Fallback approximation: cycle of ~365.25 - 10.875 days per year
            int delta = year - 2026;
            return new DateTime(2026, 2, 17).AddDays(delta * -10.875).Date;
        }
    }
}
