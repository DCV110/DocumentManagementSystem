using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        [ForeignKey("CreatedBy")]
        public virtual ApplicationUser? Creator { get; set; }

        public int? CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int TimeLimitMinutes { get; set; } = 0; // 0 = không giới hạn

        public int MaxAttempts { get; set; } = 0; // 0 = không giới hạn

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedDate { get; set; }

        // Navigation properties
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<StudentQuizAttempt> Attempts { get; set; } = new List<StudentQuizAttempt>();
    }
}

