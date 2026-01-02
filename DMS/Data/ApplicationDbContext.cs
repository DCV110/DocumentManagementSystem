using DMS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Quan trọng: Phải có using này
using Microsoft.EntityFrameworkCore;

namespace DMS.Data
{
    
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Các cấu hình Fluent API khác nếu có...
        }
    }
}