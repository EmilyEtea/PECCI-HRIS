using System.ComponentModel.DataAnnotations;

namespace PECCI_HRIS.Models
{
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }

        [Required, MaxLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? DepartmentCode { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    }
}
