using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PECCI_HRIS.Models
{
    /// <summary>
    /// Defines a recurring deduction schedule for an employee.
    /// Each active schedule automatically generates EmployeeDeduction entries
    /// for every cutoff period until the schedule ends or is stopped.
    /// </summary>
    public class RecurringDeductionSchedule
    {
        [Key]
        public int ScheduleID { get; set; }

        public int EmployeeID { get; set; }

        [Required, MaxLength(50)]
        public string DeductionType { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal AmountPerCutoff { get; set; }

        /// <summary>
        /// Which cutoff(s) to apply: "1-15", "16-30", or "Both"
        /// </summary>
        [Required, MaxLength(10)]
        public string CutoffPeriod { get; set; } = "Both";

        /// <summary>First cutoff period to apply (inclusive)</summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>Last cutoff period to apply (inclusive). Null = no end date.</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Total number of deductions to make. Null = unlimited.</summary>
        public int? TotalInstallments { get; set; }

        /// <summary>How many installments have been applied so far.</summary>
        public int AppliedInstallments { get; set; } = 0;

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Paused, Completed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        [ForeignKey("EmployeeID")]
        public virtual Employee? Employee { get; set; }

        // ── Computed helpers ──────────────────────────────────────────────────────
        [NotMapped]
        public int? RemainingInstallments =>
            TotalInstallments.HasValue ? TotalInstallments - AppliedInstallments : null;

        [NotMapped]
        public decimal TotalAmount =>
            TotalInstallments.HasValue ? AmountPerCutoff * TotalInstallments.Value : 0;

        [NotMapped]
        public decimal AmountApplied => AmountPerCutoff * AppliedInstallments;

        [NotMapped]
        public decimal AmountRemaining =>
            TotalInstallments.HasValue ? TotalAmount - AmountApplied : 0;
    }
}
