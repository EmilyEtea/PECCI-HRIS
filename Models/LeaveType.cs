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

        [MaxLength(300)]
        public string? Description { get; set; }

        public int DefaultDaysPerYear { get; set; } = 0;

        public bool IsPaid { get; set; } = true;

        public bool RequiresApproval { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual ICollection<LeaveApplication> LeaveApplications { get; set; } = new List<LeaveApplication>();
        public virtual ICollection<LeaveCredit> LeaveCredits { get; set; } = new List<LeaveCredit>();
    }
}
