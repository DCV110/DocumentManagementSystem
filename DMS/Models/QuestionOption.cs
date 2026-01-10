using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class QuestionOption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question? Question { get; set; }

        [Required]
        [StringLength(500)]
        public string OptionText { get; set; } = string.Empty;

        [StringLength(10)]
        public string? OptionLabel { get; set; } // A, B, C, D...

        public bool IsCorrect { get; set; } = false;

        public int OptionOrder { get; set; } = 1;

        // Navigation properties
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}

