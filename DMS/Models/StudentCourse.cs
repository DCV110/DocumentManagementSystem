using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class StudentCourse
    {
        [Key]
        public int Id { get; set; }
        
        // Sinh viên
        [Required]
        public string StudentId { get; set; } = string.Empty;
        [ForeignKey("StudentId")]
        public virtual ApplicationUser Student { get; set; } = null!;
        
        // Khóa học (môn học + lớp)
        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;
        
        // Ngày đăng ký
        public DateTime EnrolledDate { get; set; } = DateTime.Now;
    }
}

