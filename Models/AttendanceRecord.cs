using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PECCI_HRIS.Models
{
    public class AttendanceRecord
    {
        [Key]
        public int AttendanceID { get; set; }

        public int EmployeeID { get; set; }

        [Required]
        public DateTime AttendanceDate { get; set; }

        public TimeSpan? TimeIn { get; set; }

        public TimeSpan? TimeOut { get; set; }

        public TimeSpan? BreakOut { get; set; }

        public TimeSpan? BreakIn { get; set; }

        // Computed fields
        public double? TotalHoursWorked { get; set; }

        public double? LateMinutes { get; set; }

        public double? OvertimeMinutes { get; set; }

        public double? UndertimeMinutes { get; set; }

        [MaxLength(20)]
        public string AttendanceStatus { get; set; } = "Present"; // Present, Absent, Late, Half-day, On Leave, Holiday

        public bool IsManualEntry { get; set; } = false;

        [MaxLength(300)]
        public string? Remarks { get; set; }

        public int? AdjustedBy { get; set; }

        public DateTime? AdjustedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("EmployeeID")]
        public virtual Employee? Employee { get; set; }

        [NotMapped]
        public bool IsLate => LateMinutes.HasValue && LateMinutes > 0;

        [NotMapped]
        public bool HasOvertime => OvertimeMinutes.HasValue && OvertimeMinutes > 0;
    }
}
