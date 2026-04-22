using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PECCI_HRIS.Models
{
    public class Position
    {
        [Key]
        public int PositionID { get; set; }

        [Required, MaxLength(100)]
        public string PositionTitle { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PositionCode { get; set; }

        public int DepartmentID { get; set; }

        public decimal BasicSalary { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("DepartmentID")]
        public virtual Department? Department { get; set; }

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
