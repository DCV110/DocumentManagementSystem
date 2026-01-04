using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Message { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [StringLength(50)]
        public string? Type { get; set; } // "info", "success", "warning", "error", "approval", "rejection", etc.

        [StringLength(100)]
        public string? ActionUrl { get; set; } // URL để điều hướng khi click vào notification

        public bool IsRead { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ReadDate { get; set; }

        // Optional: Link to related entity
        [StringLength(50)]
        public string? EntityType { get; set; } // "Document", "Course", "User", etc.
        public int? EntityId { get; set; }
    }
}

