using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PECCI_HRIS.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public int RoleID { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        public int? EmployeeID { get; set; }

        // Navigation
        [ForeignKey("RoleID")]
        public virtual Role? Role { get; set; }

        [ForeignKey("EmployeeID")]
        public virtual Employee? Employee { get; set; }
    }
}
