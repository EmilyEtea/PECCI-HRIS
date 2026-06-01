using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PECCI_HRIS.Models
{
    /// <summary>
    /// Represents a formal overtime request submitted by an employee
    /// before the overtime is worked.
    ///
    /// Approval flow mirrors LeaveApplication:
    ///   Employee submits → Manager approves (Pending HR)
    ///                    → HR Admin/Staff gives final approval (Approved)
    ///   Either stage can Disapprove or the employee can Cancel while Pending.
    /// </summary>
    public class OvertimeRequest
    {
        [Key]
        public int OvertimeRequestID { get; set; }

        public int EmployeeID { get; set; }

        /// <summary>The date on which the overtime will be / was worked.</summary>
        [Required]
        public DateTime OvertimeDate { get; set; }

        /// <summary>Planned OT start time (usually = official work end time).</summary>
        [Required]
        public TimeSpan StartTime { get; set; }

        /// <summary>Planned OT end time.</summary>
        [Required]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Minutes requested = EndTime − StartTime.
        /// Computed and stored on submit so it is immutable after approval.
        /// </summary>
        public double RequestedMinutes { get; set; }

        /// <summary>
        /// Minutes actually approved (HR may reduce the requested amount).
        /// Null until HR gives final approval.
        /// </summary>
        public double? ApprovedMinutes { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
        // Pending | Pending HR | Approved | Disapproved | Cancelled

        // ── Manager approval ──────────────────────────────────────────────────
        public int? ManagerApproverID { get; set; }
        public DateTime? ManagerApprovedAt { get; set; }
        [MaxLength(300)] public string? ManagerRemarks { get; set; }

        // ── HR approval ───────────────────────────────────────────────────────
        public int? HRApproverID { get; set; }
        public DateTime? HRApprovedAt { get; set; }
        [MaxLength(300)] public string? HRRemarks { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────
        [ForeignKey("EmployeeID")]
        public virtual Employee? Employee { get; set; }

        // ── Computed helpers ──────────────────────────────────────────────────
        [NotMapped]
        public double RequestedHours => RequestedMinutes / 60.0;

        [NotMapped]
        public double? ApprovedHours => ApprovedMinutes.HasValue ? ApprovedMinutes / 60.0 : null;
    }
}
