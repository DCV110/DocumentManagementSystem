using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DMS.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(255)]
        public required string Title { get; set; }        // Tiêu đề tài liệu hiển thị
        public string? Description { get; set; }
        public required string FileName { get; set; }     // Tên file lưu trên ổ đĩa
        public required string FilePath { get; set; }     // Đường dẫn file
        public long FileSize { get; set; }       // Kích thước file (bytes)
        public required string ContentType { get; set; }   // Định dạng (.pdf, .docx...)
        public DateTime UploadDate { get; set; } = DateTime.Now;

        // Status & Approval
        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
        public string? RejectionReason { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        // Tracking
        public int ViewCount { get; set; } = 0;
        public int DownloadCount { get; set; } = 0;

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }

        // Public Sharing
        public bool IsPublicShared { get; set; } = false;
        public string? PublicShareToken { get; set; }  // Token để tạo link chia sẻ
        public bool PublicShareRequested { get; set; } = false;  // Đang chờ phê duyệt
        public bool PublicShareApproved { get; set; } = false;  // Đã được phê duyệt

        // Khóa ngoại liên kết
        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        public required string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public int? FolderId { get; set; }
        [ForeignKey("FolderId")]
        public virtual Folder? Folder { get; set; }
    }
}
