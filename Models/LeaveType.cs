using System.ComponentModel.DataAnnotations;

namespace PECCI_HRIS.Models
{
    public class LeaveType
    {
        [Key]
        public int LeaveTypeID { get; set; }

        [Required, MaxLength(50)]
        public string LeaveTypeName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? LeaveCode { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int DefaultDaysPerYear { get; set; } = 0;

        public bool IsPaid { get; set; } = true;

        public bool RequiresApproval { get; set; } = true;

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Restricts this leave type to a specific gender.
        /// Null  = available to all employees (default).
        /// "Female" = only female employees may apply (e.g. Magna Carta Leave).
        /// "Male"   = only male employees may apply (e.g. Paternity Leave).
        /// </summary>
        [MaxLength(10)]
        public string? GenderRestriction { get; set; }

        /// <summary>
        /// When true the leave date must match a specific calendar rule:
        ///   Birthday Leave — StartDate must equal the employee's birthday (month+day in the current year).
        /// Enforced in LeaveController.Apply.
        /// </summary>
        public bool IsDateSpecific { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual ICollection<LeaveApplication> LeaveApplications { get; set; } = new List<LeaveApplication>();
        public virtual ICollection<LeaveCredit> LeaveCredits { get; set; } = new List<LeaveCredit>();
    }
}

