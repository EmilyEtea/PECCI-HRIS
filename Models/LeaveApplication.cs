using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PECCI_HRIS.Models
{
    public class LeaveApplication
    {
        [Key]
        public int LeaveApplicationID { get; set; }

        public int EmployeeID { get; set; }

        public int LeaveTypeID { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public decimal NumberOfDays { get; set; }

        /// <summary>True when the employee is applying for a half day (0.5).</summary>
        public bool IsHalfDay { get; set; } = false;

        /// <summary>"AM" or "PM" — only relevant when IsHalfDay is true.</summary>
        [MaxLength(2)]
        public string? HalfDayPeriod { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Disapproved, Cancelled

        // Approval workflow
        public int? ManagerApproverID { get; set; }

        public DateTime? ManagerApprovedAt { get; set; }

        [MaxLength(300)]
        public string? ManagerRemarks { get; set; }

        public int? HRApproverID { get; set; }

        public DateTime? HRApprovedAt { get; set; }

        [MaxLength(300)]
        public string? HRRemarks { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("EmployeeID")]
        public virtual Employee? Employee { get; set; }

        [ForeignKey("LeaveTypeID")]
        public virtual LeaveType? LeaveType { get; set; }
    }
}
