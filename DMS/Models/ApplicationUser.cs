using Microsoft.AspNetCore.Identity;

namespace DMS.Models
{
    
    public class ApplicationUser : IdentityUser
    {
        public required string FullName { get; set; }
        public string? StudentCode { get; set; } // Mã số sinh viên
        public string? Faculty { get; set; }     // Khoa
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Quan hệ: Một người dùng có thể đăng nhiều tài liệu
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        
        // Quan hệ: Một sinh viên có thể đăng ký nhiều khóa học
        public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    }
}