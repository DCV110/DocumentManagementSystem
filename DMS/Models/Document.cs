using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DMS.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(255)]
        public string Title { get; set; }        // Tiêu đề tài liệu hiển thị
        public string? Description { get; set; }
        public string FileName { get; set; }     // Tên file lưu trên ổ đĩa
        public string FilePath { get; set; }     // Đường dẫn file
        public long FileSize { get; set; }       // Kích thước file (bytes)
        public string ContentType { get; set; }   // Định dạng (.pdf, .docx...)
        public DateTime UploadDate { get; set; } = DateTime.Now;

        // Khóa ngoại liên kết
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public int? FolderId { get; set; }
        [ForeignKey("FolderId")]
        public virtual Folder? Folder { get; set; }
    }
}
