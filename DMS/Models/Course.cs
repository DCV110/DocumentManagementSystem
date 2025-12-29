using System.ComponentModel.DataAnnotations;
namespace DMS.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100)]
        public required string CourseName { get; set; }   // Tên môn học
        [Required, StringLength(20)]
        public required string CourseCode { get; set; }   // Mã môn học (ví dụ: IT101)
        public string? Description { get; set; }

        // Quan hệ: Một môn học có nhiều tài liệu
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
