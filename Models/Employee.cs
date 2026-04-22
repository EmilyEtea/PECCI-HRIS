using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PECCI_HRIS.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }

        [Required, MaxLength(20)]
        public string EmployeeNo { get; set; } = string.Empty;

        // Personal Information
        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Suffix { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required, MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? CivilStatus { get; set; }

        [MaxLength(20)]
        public string? Nationality { get; set; } = "Filipino";

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [MaxLength(100)]
        public string? PersonalEmail { get; set; }

        [MaxLength(100)]
        public string? CompanyEmail { get; set; }

        // Government IDs
        [MaxLength(20)]
        public string? SSSNumber { get; set; }

        [MaxLength(20)]
        public string? PhilHealthNumber { get; set; }

        [MaxLength(20)]
        public string? PagIbigNumber { get; set; }

        [MaxLength(20)]
        public string? TINNumber { get; set; }

        // Employment Information
        public int DepartmentID { get; set; }

        public int PositionID { get; set; }

        [Required]
        public DateTime DateHired { get; set; }

        public DateTime? DateRegularized { get; set; }

        public DateTime? DateSeparated { get; set; }

        [Required, MaxLength(30)]
        public string EmploymentStatus { get; set; } = "Regular"; // Regular, Probationary, Contractual, Part-time

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Inactive, Resigned, Terminated, Retired

        [MaxLength(200)]
        public string? ProfilePicturePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedBy { get; set; }

        // Navigation
        [ForeignKey("DepartmentID")]
        public virtual Department? Department { get; set; }

        [ForeignKey("PositionID")]
        public virtual Position? Position { get; set; }

        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public virtual ICollection<LeaveApplication> LeaveApplications { get; set; } = new List<LeaveApplication>();
        public virtual ICollection<LeaveCredit> LeaveCredits { get; set; } = new List<LeaveCredit>();
        public virtual ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();
        public virtual User? UserAccount { get; set; }

        [NotMapped]
        public string FullName => $"{LastName}, {FirstName} {MiddleName}".Trim();

        [NotMapped]
        public string DisplayName => $"{FirstName} {LastName}";

        [NotMapped]
        public int Age => DateTime.Today.Year - DateOfBirth.Year -
                          (DateTime.Today < DateOfBirth.AddYears(DateTime.Today.Year - DateOfBirth.Year) ? 1 : 0);
    }
}
