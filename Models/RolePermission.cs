using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PECCI_HRIS.Models
{
    public class RolePermission
    {
        [Key]
        public int PermissionID { get; set; }

        public int RoleID { get; set; }

        [Required, MaxLength(100)]
        public string Module { get; set; } = string.Empty;

        public bool CanView { get; set; } = false;
        public bool CanCreate { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;

        [ForeignKey("RoleID")]
        public virtual Role? Role { get; set; }
    }
}
