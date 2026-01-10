using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class StudentAnswer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AttemptId { get; set; }

        [ForeignKey("AttemptId")]
        public virtual StudentQuizAttempt? Attempt { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question? Question { get; set; }

        public int? SelectedOptionId { get; set; }

        [ForeignKey("SelectedOptionId")]
        public virtual QuestionOption? SelectedOption { get; set; }

        public bool IsCorrect { get; set; } = false;

        public int? PointsEarned { get; set; }

        public int? ManualPoints { get; set; }

        public string? TeacherComment { get; set; }
    }
}

