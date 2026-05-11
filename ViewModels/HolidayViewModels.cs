using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PECCI_HRIS.ViewModels
{
    // ── List ─────────────────────────────────────────────────────────────────

    public class HolidayIndexViewModel
    {
        public List<PECCI_HRIS.Models.Holiday> Holidays { get; set; } = new();
        public int SelectedYear { get; set; } = DateTime.Today.Year;
        public string? SelectedType { get; set; }
        public List<int> AvailableYears { get; set; } = new();
        public int RegularCount  => Holidays.Count(h => h.HolidayType == "Regular");
        public int SpecialCount  => Holidays.Count(h => h.HolidayType == "Special");
    }

    // ── Create / Edit ─────────────────────────────────────────────────────────

    public class HolidayFormViewModel
    {
        public int HolidayID { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Holiday Date")]
        public DateTime HolidayDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Holiday name is required.")]
        [MaxLength(150)]
        [Display(Name = "Holiday Name")]
        public string HolidayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Holiday type is required.")]
        [Display(Name = "Holiday Type")]
        public string HolidayType { get; set; } = "Regular";

        [Display(Name = "Recurring annually")]
        public bool IsRecurring { get; set; } = false;

        [MaxLength(300)]
        [Display(Name = "Remarks / Notes")]
        public string? Remarks { get; set; }

        public bool IsEdit => HolidayID > 0;

        public static List<SelectListItem> HolidayTypeOptions => new()
        {
            new SelectListItem("Regular Holiday (200% pay)", "Regular"),
            new SelectListItem("Special Non-Working Holiday (130% pay)", "Special")
        };
    }
}
