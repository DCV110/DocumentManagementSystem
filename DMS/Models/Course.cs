using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [StringLength(20)]
        public string? ClassCode { get; set; }   // Mã lớp (ví dụ: IT001-L01)
        public string? Description { get; set; }

        // Giảng viên phụ trách
        public string? InstructorId { get; set; }
        [ForeignKey("InstructorId")]
        public virtual ApplicationUser? Instructor { get; set; }

        // Quan hệ: Một môn học có nhiều tài liệu
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        
        // Quan hệ: Một khóa học có nhiều sinh viên đăng ký
        public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
    }
}
