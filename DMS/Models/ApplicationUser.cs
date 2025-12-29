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

    }
}