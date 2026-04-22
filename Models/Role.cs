using System.ComponentModel.DataAnnotations;

namespace PECCI_HRIS.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        [Required, MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
