using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PECCI_HRIS.Models
{
    public class EmployeeDeduction
    {
        [Key]
        public int DeductionID { get; set; }

        public int EmployeeID { get; set; }

        [Required, MaxLength(50)]
        public string DeductionType { get; set; } = string.Empty;
        // e.g. "SSS Loan", "Pag-IBIG Loan", "PECCI Loan", "Other"

        [Required, MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        // Which cutoff this applies to: "1-15" or "16-30"
        [Required, MaxLength(10)]
        public string CutoffPeriod { get; set; } = string.Empty;

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Applied, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        [ForeignKey("EmployeeID")]
        public virtual Employee? Employee { get; set; }
    }
}
