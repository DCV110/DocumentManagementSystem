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
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<BackupRecord> BackupRecords { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        
        // Quiz System
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<StudentQuizAttempt> StudentQuizAttempts { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<FavoriteDocument> FavoriteDocuments { get; set; }
        public DbSet<DocumentComment> DocumentComments { get; set; }
        public DbSet<DocumentRating> DocumentRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure Quiz relationships to avoid multiple cascade paths
            builder.Entity<StudentQuizAttempt>(entity =>
            {
                entity.HasOne(s => s.Quiz)
                    .WithMany(q => q.Attempts)
                    .HasForeignKey(s => s.QuizId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(s => s.Student)
                    .WithMany()
                    .HasForeignKey(s => s.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            builder.Entity<StudentAnswer>(entity =>
            {
                entity.HasOne(s => s.Attempt)
                    .WithMany(a => a.Answers)
                    .HasForeignKey(s => s.AttemptId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(s => s.Question)
                    .WithMany(q => q.StudentAnswers)
                    .HasForeignKey(s => s.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configure FavoriteDocument unique constraint
            builder.Entity<FavoriteDocument>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.DocumentId })
                    .IsUnique();
                
                entity.HasOne(f => f.User)
                    .WithMany()
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(f => f.Document)
                    .WithMany()
                    .HasForeignKey(f => f.DocumentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configure DocumentComment
            builder.Entity<DocumentComment>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(c => c.Document)
                    .WithMany()
                    .HasForeignKey(c => c.DocumentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configure DocumentRating - unique constraint (mỗi user chỉ đánh giá 1 lần cho 1 document)
            builder.Entity<DocumentRating>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.DocumentId })
                    .IsUnique();
                
                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(r => r.Document)
                    .WithMany()
                    .HasForeignKey(r => r.DocumentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Các cấu hình Fluent API khác nếu có...
        }
    }
}