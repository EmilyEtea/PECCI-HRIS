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

    public class RecurringDeductionViewModel
    {
        public int ScheduleID { get; set; }

        [Required(ErrorMessage = "Employee is required.")]
        [Display(Name = "Employee")]
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "Deduction type is required.")]
        [Display(Name = "Deduction Type")]
        public string DeductionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(200)]
        [Display(Name = "Description / Reference")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be greater than zero.")]
        [Display(Name = "Amount per Cutoff (₱)")]
        public decimal AmountPerCutoff { get; set; }

        [Required(ErrorMessage = "Cutoff period is required.")]
        [Display(Name = "Apply On")]
        public string CutoffPeriod { get; set; } = "Both";

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "End Date (optional)")]
        public DateTime? EndDate { get; set; }

        [Range(1, 999, ErrorMessage = "Must be at least 1 installment.")]
        [Display(Name = "Total Installments (optional)")]
        public int? TotalInstallments { get; set; }

        public IEnumerable<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
    }

    public class RecurringDeductionListViewModel
    {
        public int ScheduleID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNo { get; set; } = string.Empty;
        public string DeductionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal AmountPerCutoff { get; set; }
        public string CutoffPeriod { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TotalInstallments { get; set; }
        public int AppliedInstallments { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
