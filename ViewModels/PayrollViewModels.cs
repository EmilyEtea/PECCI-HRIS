using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PECCI_HRIS.ViewModels
{
    public class PayrollComputeViewModel
    {
        [Required(ErrorMessage = "Cutoff period is required.")]
        [RegularExpression(@"^(1-15|16-30)$", ErrorMessage = "Cutoff period must be '1-15' or '16-30'.")]
        [Display(Name = "Cutoff Period")]
        public string CutoffPeriod { get; set; } = string.Empty;

        [Required(ErrorMessage = "Month is required.")]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
        [Display(Name = "Month")]
        public int Month { get; set; } = DateTime.Today.Month;

        [Required(ErrorMessage = "Year is required.")]
        [Range(2020, 2099, ErrorMessage = "Year must be between 2020 and 2099.")]
        [Display(Name = "Year")]
        public int Year { get; set; } = DateTime.Today.Year;

        public int? EmployeeID { get; set; }

        public IEnumerable<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
    }

    public class PayrollSummaryViewModel
    {
        public int PayrollID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNo { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string PayPeriod { get; set; } = string.Empty;
        public decimal BasicSalary { get; set; }
        public decimal GrossPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public string Status { get; set; } = string.Empty;

        // Deduction breakdown
        public decimal SSSContribution { get; set; }
        public decimal PhilHealthContribution { get; set; }
        public decimal PagIbigContribution { get; set; }
        public decimal WithholdingTax { get; set; }
        public decimal LateDeductions { get; set; }
        public decimal UndertimeDeductions { get; set; }
        public decimal OtherDeductions { get; set; }

        // Earnings breakdown
        public decimal OvertimePay { get; set; }
        public decimal HolidayPay { get; set; }
        public decimal NightDifferential { get; set; }
        public decimal Allowances { get; set; }

        // Attendance
        public int DaysWorked { get; set; }
        public int DaysAbsent { get; set; }
        public double TotalOvertimeHours { get; set; }
        public double TotalLateMinutes { get; set; }
        public string TaxBracket { get; set; } = string.Empty;
    }

    public class PayslipViewModel : PayrollSummaryViewModel
    {
        public string CompanyName { get; set; } = "PECCI Multipurpose Cooperative";
        public string CompanyAddress { get; set; } = "4th Floor Universal RE Building, Paseo de Roxas cor. Perea St. Makati City";
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
