using System.ComponentModel.DataAnnotations;

namespace PECCI_HRIS.Models
{
    /// <summary>
    /// Represents a Philippine public holiday (regular or special non-working).
    /// Based on the annual Proclamation list issued by the Office of the President.
    /// </summary>
    public class Holiday
    {
        [Key]
        public int HolidayID { get; set; }

        /// <summary>The calendar date of the holiday.</summary>
        [Required]
        public DateTime HolidayDate { get; set; }

        /// <summary>Official name of the holiday (e.g., "New Year's Day").</summary>
        [Required, MaxLength(150)]
        public string HolidayName { get; set; } = string.Empty;

        /// <summary>
        /// "Regular" — 200% pay for work rendered (Labor Code Art. 94).
        /// "Special"  — 130% pay for work rendered (DOLE rules).
        /// </summary>
        [Required, MaxLength(20)]
        public string HolidayType { get; set; } = "Regular"; // Regular | Special

        /// <summary>Calendar year this entry belongs to.</summary>
        public int Year { get; set; }

        /// <summary>
        /// When true the holiday recurs on the same month/day every year
        /// (e.g., New Year's Day, Christmas Day).
        /// When false it is a one-off proclamation date.
        /// </summary>
        public bool IsRecurring { get; set; } = false;

        /// <summary>Optional notes (e.g., "In lieu of …", proclamation number).</summary>
        [MaxLength(300)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        // ── Computed helpers ─────────────────────────────────────────────────
        public bool IsRegular  => HolidayType == "Regular";
        public bool IsSpecial  => HolidayType == "Special";

        /// <summary>Display label used in views.</summary>
        public string TypeBadge => HolidayType == "Regular" ? "Regular Holiday" : "Special Non-Working";
    }
}
