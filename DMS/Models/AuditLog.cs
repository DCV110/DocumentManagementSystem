using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // "Create", "Update", "Delete", "View", "Approve", "Reject", etc.
        
        [StringLength(100)]
        public string? EntityType { get; set; } // "Document", "User", "Course", "Folder"
        
        public int? EntityId { get; set; } // ID của entity được thao tác
        
        [StringLength(500)]
        public string? Description { get; set; } // Mô tả chi tiết
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        // Dữ liệu bổ sung dạng JSON (nếu cần)
        public string? AdditionalData { get; set; }
    }
}

