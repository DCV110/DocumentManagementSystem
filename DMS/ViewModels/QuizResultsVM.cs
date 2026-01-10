using DMS.Models;

namespace DMS.ViewModels
{
    public class QuizResultsVM
    {
        public Quiz Quiz { get; set; } = null!;
        public List<AttemptResultVM> Attempts { get; set; } = new List<AttemptResultVM>();
        public int TotalStudents { get; set; }
        public int CompletedCount { get; set; }
        public double AverageScore { get; set; }
    }

    public class AttemptResultVM
    {
        public StudentQuizAttempt Attempt { get; set; } = null!;
        public ApplicationUser Student { get; set; } = null!;
        public List<AnswerDetailVM> Answers { get; set; } = new List<AnswerDetailVM>();
    }

    public class AnswerDetailVM
    {
        public Question Question { get; set; } = null!;
        public QuestionOption? SelectedOption { get; set; }
        public QuestionOption? CorrectOption { get; set; }
        public bool IsCorrect { get; set; }
        public int? PointsEarned { get; set; }
        public int? ManualPoints { get; set; }
        public string? TeacherComment { get; set; }
    }

    public class AttemptDetailsVM
    {
        public StudentQuizAttempt Attempt { get; set; } = null!;
        public Quiz Quiz { get; set; } = null!;
        public ApplicationUser Student { get; set; } = null!;
    }
}

