using System.ComponentModel.DataAnnotations;

namespace PECCI_HRIS.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditID { get; set; }

        public int? UserID { get; set; }

        [MaxLength(50)]
        public string? Username { get; set; }

        [Required, MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Module { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? IPAddress { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [MaxLength(1000)]
        public string? OldValues { get; set; }

        [MaxLength(1000)]
        public string? NewValues { get; set; }
    }
}
