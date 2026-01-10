using DMS.Models;
using DMS.ViewModels;

namespace DMS.Services.Interfaces
{
    public interface IQuizService
    {
        // Quiz Management
        Task<Quiz> CreateQuizAsync(QuizCreateVM model, string instructorId);
        Task<Quiz?> GetQuizByIdAsync(int id);
        Task<Quiz?> GetQuizWithDetailsAsync(int id);
        Task<List<Quiz>> GetQuizzesByInstructorAsync(string instructorId);
        Task<List<Quiz>> GetAllQuizzesAsync(); // For admin
        Task<List<Quiz>> GetQuizzesByCourseAsync(int courseId);
        Task<List<Quiz>> GetAvailableQuizzesForStudentAsync(string studentId);
        Task<bool> UpdateQuizAsync(int id, QuizCreateVM model);
        Task<bool> DeleteQuizAsync(int id);
        Task<bool> PublishQuizAsync(int id, int? courseId);
        Task<bool> UnpublishQuizAsync(int id);
        
        // Question Management
        Task<Question> AddQuestionAsync(int quizId, QuestionCreateVM model);
        Task<Question?> GetQuestionByIdAsync(int id);
        Task<bool> UpdateQuestionAsync(int id, QuestionCreateVM model);
        Task<bool> DeleteQuestionAsync(int id);
        Task<bool> ReorderQuestionsAsync(int quizId, List<int> questionIds);
        
        // Student Quiz Taking
        Task<int> GetAttemptCountAsync(int quizId, string studentId);
        Task<StudentQuizAttempt> StartQuizAttemptAsync(int quizId, string studentId);
        Task<StudentQuizAttempt?> GetAttemptByIdAsync(int id);
        Task<StudentQuizAttempt?> GetAttemptByQuizAndStudentAsync(int quizId, string studentId);
        Task<bool> SaveAnswerAsync(int attemptId, int questionId, int? optionId);
        Task<bool> SubmitQuizAttemptAsync(int attemptId);
        Task<(int totalScore, int maxScore, double percentage)> AutoGradeAttemptAsync(int attemptId);
        
        // Results & Grading
        Task<List<StudentQuizAttempt>> GetAttemptsByQuizAsync(int quizId);
        Task<List<StudentQuizAttempt>> GetAttemptsByStudentAsync(string studentId);
        Task<bool> ManualGradeAttemptAsync(int attemptId, int? manualScore, string? comment);
        Task<bool> ManualGradeAnswerAsync(int answerId, int? manualPoints, string? comment);
    }
}

