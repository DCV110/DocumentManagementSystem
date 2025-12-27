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

        // Khóa ngoại liên kết
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        public required string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public int? FolderId { get; set; }
        [ForeignKey("FolderId")]
        public virtual Folder? Folder { get; set; }
    }
}
