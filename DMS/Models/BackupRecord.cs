using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class BackupRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string BackupName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string DatabaseBackupPath { get; set; } = string.Empty; // Đường dẫn file backup database

        [StringLength(500)]
        public string? FilesBackupPath { get; set; } // Đường dẫn file backup files (zip)

        public long DatabaseSize { get; set; } // Kích thước file backup database (bytes)
        public long FilesSize { get; set; } // Kích thước file backup files (bytes)

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty; // UserId của admin tạo backup

        [ForeignKey("CreatedBy")]
        public virtual ApplicationUser CreatedByUser { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsRestored { get; set; } = false; // Đã được restore chưa
        public DateTime? RestoredDate { get; set; }
        public string? RestoredBy { get; set; } // UserId của admin restore

        [StringLength(500)]
        public string? RestoreNotes { get; set; }
    }
}

