using DMS.Data;
using DMS.Models;
using DMS.Services.Interfaces;
using DMS.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DMS.Services
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;

        public QuizService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Quiz Management
        public async Task<Quiz> CreateQuizAsync(QuizCreateVM model, string instructorId)
        {
            var quiz = new Quiz
            {
                Title = model.Title,
                Description = model.Description,
                CreatedBy = instructorId,
                CourseId = model.CourseId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                TimeLimitMinutes = model.TimeLimitMinutes,
                MaxAttempts = model.MaxAttempts,
                IsPublished = false,
                CreatedDate = DateTime.Now
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            // Add questions
            int questionOrder = 1;
            foreach (var questionVM in model.Questions)
            {
                var question = new Question
                {
                    QuizId = quiz.Id,
                    QuestionText = questionVM.QuestionText,
                    QuestionOrder = questionOrder++,
                    Points = questionVM.Points
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                // Add options
                int optionOrder = 1;
                char label = 'A';
                foreach (var optionVM in questionVM.Options)
                {
                    var option = new QuestionOption
                    {
                        QuestionId = question.Id,
                        OptionText = optionVM.OptionText,
                        OptionLabel = string.IsNullOrEmpty(optionVM.OptionLabel) ? label.ToString() : optionVM.OptionLabel,
                        IsCorrect = optionVM.IsCorrect,
                        OptionOrder = optionOrder++
                    };

                    _context.QuestionOptions.Add(option);
                    label++;
                }
            }

            await _context.SaveChangesAsync();
            return quiz;
        }

        public async Task<Quiz?> GetQuizByIdAsync(int id)
        {
            return await _context.Quizzes
                .Include(q => q.Creator)
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);
        }

        public async Task<Quiz?> GetQuizWithDetailsAsync(int id)
        {
            return await _context.Quizzes
                .Include(q => q.Creator)
                .Include(q => q.Course)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);
        }

        public async Task<List<Quiz>> GetQuizzesByInstructorAsync(string instructorId)
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                .Where(q => q.CreatedBy == instructorId && !q.IsDeleted)
                .OrderByDescending(q => q.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Quiz>> GetAllQuizzesAsync()
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Creator)
                .Include(q => q.Questions)
                .Where(q => !q.IsDeleted)
                .OrderByDescending(q => q.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Quiz>> GetQuizzesByCourseAsync(int courseId)
        {
            return await _context.Quizzes
                .Include(q => q.Creator)
                .Where(q => q.CourseId == courseId && q.IsPublished && !q.IsDeleted)
                .OrderByDescending(q => q.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Quiz>> GetAvailableQuizzesForStudentAsync(string studentId)
        {
            // Get courses that student is enrolled in
            var enrolledCourseIds = await _context.StudentCourses
                .Where(sc => sc.StudentId == studentId)
                .Select(sc => sc.CourseId)
                .ToListAsync();

            if (!enrolledCourseIds.Any())
            {
                return new List<Quiz>();
            }

            // Get published quizzes from enrolled courses
            var quizzes = await _context.Quizzes
                .Include(q => q.Creator)
                .Include(q => q.Course)
                .Include(q => q.Questions)
                .Where(q => q.IsPublished && 
                           !q.IsDeleted && 
                           q.CourseId.HasValue && 
                           enrolledCourseIds.Contains(q.CourseId.Value))
                .OrderByDescending(q => q.CreatedDate)
                .ToListAsync();

            // Filter by date if specified
            var now = DateTime.Now;
            return quizzes.Where(q => 
                (!q.StartDate.HasValue || q.StartDate.Value <= now) &&
                (!q.EndDate.HasValue || q.EndDate.Value >= now)
            ).ToList();
        }

        public async Task<bool> UpdateQuizAsync(int id, QuizCreateVM model)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

            if (quiz == null) return false;

            quiz.Title = model.Title;
            quiz.Description = model.Description;
            quiz.CourseId = model.CourseId;
            quiz.StartDate = model.StartDate;
            quiz.EndDate = model.EndDate;
            quiz.TimeLimitMinutes = model.TimeLimitMinutes;
            quiz.MaxAttempts = model.MaxAttempts;

            // Delete existing questions and options
            foreach (var question in quiz.Questions.ToList())
            {
                _context.QuestionOptions.RemoveRange(question.Options);
                _context.Questions.Remove(question);
            }

            // Add new questions
            int questionOrder = 1;
            foreach (var questionVM in model.Questions)
            {
                var question = new Question
                {
                    QuizId = quiz.Id,
                    QuestionText = questionVM.QuestionText,
                    QuestionOrder = questionOrder++,
                    Points = questionVM.Points
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                int optionOrder = 1;
                char label = 'A';
                foreach (var optionVM in questionVM.Options)
                {
                    var option = new QuestionOption
                    {
                        QuestionId = question.Id,
                        OptionText = optionVM.OptionText,
                        OptionLabel = string.IsNullOrEmpty(optionVM.OptionLabel) ? label.ToString() : optionVM.OptionLabel,
                        IsCorrect = optionVM.IsCorrect,
                        OptionOrder = optionOrder++
                    };

                    _context.QuestionOptions.Add(option);
                    label++;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuizAsync(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null) return false;

            quiz.IsDeleted = true;
            quiz.DeletedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishQuizAsync(int id, int? courseId)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null) return false;

            quiz.IsPublished = true;
            quiz.CourseId = courseId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnpublishQuizAsync(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null) return false;

            quiz.IsPublished = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // Question Management
        public async Task<Question> AddQuestionAsync(int quizId, QuestionCreateVM model)
        {
            var maxOrder = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .MaxAsync(q => (int?)q.QuestionOrder) ?? 0;

            var question = new Question
            {
                QuizId = quizId,
                QuestionText = model.QuestionText,
                QuestionOrder = maxOrder + 1,
                Points = model.Points
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            int optionOrder = 1;
            char label = 'A';
            foreach (var optionVM in model.Options)
            {
                var option = new QuestionOption
                {
                    QuestionId = question.Id,
                    OptionText = optionVM.OptionText,
                    OptionLabel = string.IsNullOrEmpty(optionVM.OptionLabel) ? label.ToString() : optionVM.OptionLabel,
                    IsCorrect = optionVM.IsCorrect,
                    OptionOrder = optionOrder++
                };

                _context.QuestionOptions.Add(option);
                label++;
            }

            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<Question?> GetQuestionByIdAsync(int id)
        {
            return await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<bool> UpdateQuestionAsync(int id, QuestionCreateVM model)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null) return false;

            question.QuestionText = model.QuestionText;
            question.Points = model.Points;

            // Delete existing options
            _context.QuestionOptions.RemoveRange(question.Options);

            // Add new options
            int optionOrder = 1;
            char label = 'A';
            foreach (var optionVM in model.Options)
            {
                var option = new QuestionOption
                {
                    QuestionId = question.Id,
                    OptionText = optionVM.OptionText,
                    OptionLabel = string.IsNullOrEmpty(optionVM.OptionLabel) ? label.ToString() : optionVM.OptionLabel,
                    IsCorrect = optionVM.IsCorrect,
                    OptionOrder = optionOrder++
                };

                _context.QuestionOptions.Add(option);
                label++;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuestionAsync(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null) return false;

            _context.QuestionOptions.RemoveRange(question.Options);
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderQuestionsAsync(int quizId, List<int> questionIds)
        {
            var questions = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .ToListAsync();

            for (int i = 0; i < questionIds.Count; i++)
            {
                var question = questions.FirstOrDefault(q => q.Id == questionIds[i]);
                if (question != null)
                {
                    question.QuestionOrder = i + 1;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Student Quiz Taking
        public async Task<int> GetAttemptCountAsync(int quizId, string studentId)
        {
            return await _context.StudentQuizAttempts
                .CountAsync(a => a.QuizId == quizId && a.StudentId == studentId && a.SubmittedAt != null);
        }

        public async Task<StudentQuizAttempt> StartQuizAttemptAsync(int quizId, string studentId)
        {
            // Check if student already has an attempt
            var existingAttempt = await _context.StudentQuizAttempts
                .FirstOrDefaultAsync(a => a.QuizId == quizId && a.StudentId == studentId && a.SubmittedAt == null);

            if (existingAttempt != null)
            {
                return existingAttempt;
            }

            // Check max attempts
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz != null && quiz.MaxAttempts > 0)
            {
                var attemptCount = await GetAttemptCountAsync(quizId, studentId);
                if (attemptCount >= quiz.MaxAttempts)
                {
                    throw new InvalidOperationException($"Bạn đã sử dụng hết số lần làm bài ({quiz.MaxAttempts} lần).");
                }
            }

            var attempt = new StudentQuizAttempt
            {
                QuizId = quizId,
                StudentId = studentId,
                StartedAt = DateTime.Now
            };

            _context.StudentQuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();
            return attempt;
        }

        public async Task<StudentQuizAttempt?> GetAttemptByIdAsync(int id)
        {
            return await _context.StudentQuizAttempts
                .Include(a => a.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Options)
                .Include(a => a.Student)
                .Include(a => a.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<StudentQuizAttempt?> GetAttemptByQuizAndStudentAsync(int quizId, string studentId)
        {
            return await _context.StudentQuizAttempts
                .Include(a => a.Quiz)
                .Include(a => a.Student)
                .Include(a => a.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .OrderByDescending(a => a.StartedAt)
                .FirstOrDefaultAsync(a => a.QuizId == quizId && a.StudentId == studentId);
        }

        public async Task<bool> SaveAnswerAsync(int attemptId, int questionId, int? optionId)
        {
            var answer = await _context.StudentAnswers
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.QuestionId == questionId);

            if (answer == null)
            {
                answer = new StudentAnswer
                {
                    AttemptId = attemptId,
                    QuestionId = questionId,
                    SelectedOptionId = optionId
                };
                _context.StudentAnswers.Add(answer);
            }
            else
            {
                answer.SelectedOptionId = optionId;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitQuizAttemptAsync(int attemptId)
        {
            var attempt = await _context.StudentQuizAttempts
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null || attempt.SubmittedAt != null) return false;

            attempt.SubmittedAt = DateTime.Now;

            // Auto-grade
            var (totalScore, maxScore, percentage) = await AutoGradeAttemptAsync(attemptId);
            attempt.TotalScore = totalScore;
            attempt.MaxScore = maxScore;
            attempt.ScorePercentage = percentage;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(int totalScore, int maxScore, double percentage)> AutoGradeAttemptAsync(int attemptId)
        {
            var attempt = await _context.StudentQuizAttempts
                .Include(a => a.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Options)
                .Include(a => a.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return (0, 0, 0);

            int totalScore = 0;
            int maxScore = attempt.Quiz.Questions.Sum(q => q.Points);

            foreach (var question in attempt.Quiz.Questions)
            {
                var answer = attempt.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);

                if (answer != null && answer.SelectedOptionId.HasValue)
                {
                    var selectedOption = question.Options.FirstOrDefault(o => o.Id == answer.SelectedOptionId.Value);
                    bool isCorrect = selectedOption != null && selectedOption.IsCorrect;

                    answer.IsCorrect = isCorrect;
                    answer.PointsEarned = isCorrect ? question.Points : 0;
                    totalScore += answer.PointsEarned.Value;
                }
                else
                {
                    answer = new StudentAnswer
                    {
                        AttemptId = attemptId,
                        QuestionId = question.Id,
                        IsCorrect = false,
                        PointsEarned = 0
                    };
                    _context.StudentAnswers.Add(answer);
                }
            }

            double percentage = maxScore > 0 ? (totalScore / (double)maxScore) * 100 : 0;
            await _context.SaveChangesAsync();

            return (totalScore, maxScore, percentage);
        }

        // Results & Grading
        public async Task<List<StudentQuizAttempt>> GetAttemptsByQuizAsync(int quizId)
        {
            return await _context.StudentQuizAttempts
                .Include(a => a.Student)
                .Include(a => a.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .Where(a => a.QuizId == quizId && a.SubmittedAt != null)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<StudentQuizAttempt>> GetAttemptsByStudentAsync(string studentId)
        {
            return await _context.StudentQuizAttempts
                .Include(a => a.Quiz)
                    .ThenInclude(q => q.Course)
                .Where(a => a.StudentId == studentId && a.SubmittedAt != null)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }

        public async Task<bool> ManualGradeAttemptAsync(int attemptId, int? manualScore, string? comment)
        {
            var attempt = await _context.StudentQuizAttempts.FindAsync(attemptId);
            if (attempt == null) return false;

            attempt.ManualScore = manualScore;
            attempt.TeacherComment = comment;
            attempt.IsGraded = true;
            attempt.GradedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ManualGradeAnswerAsync(int answerId, int? manualPoints, string? comment)
        {
            var answer = await _context.StudentAnswers.FindAsync(answerId);
            if (answer == null) return false;

            answer.ManualPoints = manualPoints;
            answer.TeacherComment = comment;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

