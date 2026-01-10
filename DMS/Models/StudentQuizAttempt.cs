using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class StudentQuizAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz? Quiz { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey("StudentId")]
        public virtual ApplicationUser? Student { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.Now;

        public DateTime? SubmittedAt { get; set; }

        public int? TotalScore { get; set; }

        public int? MaxScore { get; set; }

        public double? ScorePercentage { get; set; }

        public int? ManualScore { get; set; }

        public string? TeacherComment { get; set; }

        public bool IsGraded { get; set; } = false;

        public DateTime? GradedAt { get; set; }

        // Navigation properties
        public virtual ICollection<StudentAnswer> Answers { get; set; } = new List<StudentAnswer>();
    }
}

