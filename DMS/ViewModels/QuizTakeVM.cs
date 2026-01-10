using DMS.Models;

namespace DMS.ViewModels
{
    public class QuizTakeVM
    {
        public Quiz Quiz { get; set; } = null!;
        public StudentQuizAttempt Attempt { get; set; } = null!;
        public List<QuestionWithOptionsVM> Questions { get; set; } = new List<QuestionWithOptionsVM>();
        public Dictionary<int, int?> SelectedAnswers { get; set; } = new Dictionary<int, int?>();
        public int TimeRemainingSeconds { get; set; } = 0;
        public int? RemainingAttempts { get; set; }
    }

    public class QuestionWithOptionsVM
    {
        public Question Question { get; set; } = null!;
        public List<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        public int? SelectedOptionId { get; set; }
    }
}

