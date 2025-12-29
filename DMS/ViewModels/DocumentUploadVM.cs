using System.ComponentModel.DataAnnotations;

namespace DMS.ViewModels
{
    public class DocumentUploadVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [Display(Name = "Tiêu đề tài liệu")]
        public required string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn môn học")]
        [Display(Name = "Môn học")]
        public int CourseId { get; set; }

        [Display(Name = "Thư mục")]
        public int? FolderId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn file")]
        [Display(Name = "File tài liệu")]
        public required IFormFile File { get; set; }

        [Display(Name = "Tags (phân cách bằng dấu phẩy)")]
        public string? Tags { get; set; }
    }
}
