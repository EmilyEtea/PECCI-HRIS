using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PECCI_HRIS.ViewModels
{
    public class DeductionViewModel
    {
        public int DeductionID { get; set; }

        [Required(ErrorMessage = "Employee is required.")]
        [Display(Name = "Employee")]
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "Deduction type is required.")]
        [Display(Name = "Deduction Type")]
        public string DeductionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(200)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be greater than zero.")]
        [Display(Name = "Amount (₱)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Cutoff period is required.")]
        [RegularExpression(@"^(1-15|16-30)$", ErrorMessage = "Cutoff period must be '1-15' or '16-30'.")]
        [Display(Name = "Cutoff Period")]
        public string CutoffPeriod { get; set; } = string.Empty;

        [Required]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
        [Display(Name = "Month")]
        public int Month { get; set; } = DateTime.Today.Month;

        [Required]
        [Range(2020, 2099, ErrorMessage = "Year must be between 2020 and 2099.")]
        [Display(Name = "Year")]
        public int Year { get; set; } = DateTime.Today.Year;

        // Dropdowns
        public IEnumerable<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
    }

    public class DeductionListViewModel
    {
        public int DeductionID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNo { get; set; } = string.Empty;
        public string DeductionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CutoffPeriod { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
