using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz? Quiz { get; set; }

        [Required]
        [StringLength(1000)]
        public string QuestionText { get; set; } = string.Empty;

        public int Points { get; set; } = 1;

        public int QuestionOrder { get; set; } = 1;

        // Navigation properties
        public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}

