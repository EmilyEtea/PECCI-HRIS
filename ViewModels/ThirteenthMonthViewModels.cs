using System.ComponentModel.DataAnnotations;

namespace PECCI_HRIS.ViewModels
{
    /// <summary>
    /// Parameters for computing 13th month pay.
    /// Per PD 851: 13th month = total basic salary earned in the year / 12.
    /// Only employees who worked at least 1 month are entitled.
    /// </summary>
    public class ThirteenthMonthComputeViewModel
    {
        [Required]
        [Range(2020, 2099, ErrorMessage = "Enter a valid year.")]
        [Display(Name = "Year")]
        public int Year { get; set; } = DateTime.Today.Year;

        /// <summary>Optional — compute for a single employee only.</summary>
        [Display(Name = "Employee (leave blank for all)")]
        public int? EmployeeID { get; set; }

        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Employees { get; set; }
            = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
    }

    /// <summary>
    /// Result row for one employee's 13th month computation.
    /// </summary>
    public class ThirteenthMonthResultRow
    {
        public int     EmployeeID     { get; set; }
        public string  EmployeeNo     { get; set; } = string.Empty;
        public string  EmployeeName   { get; set; } = string.Empty;
        public string  Department     { get; set; } = string.Empty;
        public string  Position       { get; set; } = string.Empty;
        public int     Year           { get; set; }

        /// <summary>Total basic salary earned across all finalized payroll records for the year.</summary>
        public decimal TotalBasicEarned { get; set; }

        /// <summary>Number of months with at least one finalized payroll record.</summary>
        public int MonthsWorked { get; set; }

        /// <summary>
        /// 13th month pay = TotalBasicEarned / 12.
        /// Per DOLE: based on total basic salary for the calendar year, divided by 12.
        /// </summary>
        public decimal ThirteenthMonthPay => Math.Round(TotalBasicEarned / 12m, 2);

        /// <summary>Tax-exempt portion: up to ₱90,000 is exempt from withholding tax (TRAIN Law).</summary>
        public decimal TaxExemptAmount => Math.Min(ThirteenthMonthPay, 90_000m);

        /// <summary>Amount subject to tax if 13th month exceeds ₱90,000.</summary>
        public decimal TaxableAmount => Math.Max(0m, ThirteenthMonthPay - 90_000m);
    }

    /// <summary>
    /// Full result page view model — list of rows + summary totals.
    /// </summary>
    public class ThirteenthMonthResultViewModel
    {
        public int Year { get; set; }
        public List<ThirteenthMonthResultRow> Rows { get; set; } = new();

        public decimal TotalPayout    => Rows.Sum(r => r.ThirteenthMonthPay);
        public int     TotalEmployees => Rows.Count;
    }
}
