using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PECCI_HRIS.Models
{
    public class LeaveCredit
    {
        [Key]
        public int LeaveCreditID { get; set; }

        public int EmployeeID { get; set; }

        public int LeaveTypeID { get; set; }

        public int Year { get; set; }

        public decimal TotalCredits { get; set; }

        public decimal UsedCredits { get; set; } = 0;

        public decimal PendingCredits { get; set; } = 0;

        [NotMapped]
        public decimal RemainingCredits => TotalCredits - UsedCredits - PendingCredits;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("EmployeeID")]
        public virtual Employee? Employee { get; set; }

        [ForeignKey("LeaveTypeID")]
        public virtual LeaveType? LeaveType { get; set; }
    }
}
