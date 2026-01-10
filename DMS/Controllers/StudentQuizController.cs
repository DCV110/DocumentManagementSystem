using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DMS.Models;
using DMS.Services.Interfaces;
using DMS.ViewModels;

namespace DMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentQuizController : Controller
    {
        private readonly IQuizService _quizService;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentQuizController(
            IQuizService quizService,
            UserManager<ApplicationUser> userManager)
        {
            _quizService = quizService;
            _userManager = userManager;
        }

        // GET: Available Quizzes
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var quizzes = await _quizService.GetAvailableQuizzesForStudentAsync(user.Id);
            ViewBag.Quizzes = quizzes;
            return View();
        }

        // GET: Take Quiz
        public async Task<IActionResult> Take(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var quiz = await _quizService.GetQuizWithDetailsAsync(id);
            if (quiz == null || !quiz.IsPublished)
            {
                TempData["ErrorMessage"] = "Bài kiểm tra không tồn tại hoặc chưa được công bố";
                return RedirectToAction("Index");
            }

            // Check if quiz is available
            var now = DateTime.Now;
            if (quiz.StartDate.HasValue && quiz.StartDate.Value > now)
            {
                TempData["ErrorMessage"] = "Bài kiểm tra chưa bắt đầu";
                return RedirectToAction("Index");
            }

            if (quiz.EndDate.HasValue && quiz.EndDate.Value < now)
            {
                TempData["ErrorMessage"] = "Bài kiểm tra đã kết thúc";
                return RedirectToAction("Index");
            }

            // Check max attempts
            var attemptCount = await _quizService.GetAttemptCountAsync(id, user.Id);
            if (quiz.MaxAttempts > 0 && attemptCount >= quiz.MaxAttempts)
            {
                TempData["ErrorMessage"] = $"Bạn đã sử dụng hết số lần làm bài ({quiz.MaxAttempts} lần).";
                return RedirectToAction("Index");
            }

            // Start or get existing attempt
            StudentQuizAttempt attempt;
            try
            {
                attempt = await _quizService.StartQuizAttemptAsync(id, user.Id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }

            // Check if already submitted
            if (attempt.SubmittedAt.HasValue)
            {
                TempData["InfoMessage"] = "Bạn đã nộp bài kiểm tra này rồi";
                return RedirectToAction("ViewResult", new { attemptId = attempt.Id });
            }

            // Get questions with options
            var questions = quiz.Questions.OrderBy(q => q.QuestionOrder).ToList();
            var questionVMs = questions.Select(q => new QuestionWithOptionsVM
            {
                Question = q,
                Options = q.Options.OrderBy(o => o.OptionOrder).ToList(),
                SelectedOptionId = attempt.Answers.FirstOrDefault(a => a.QuestionId == q.Id)?.SelectedOptionId
            }).ToList();

            // Calculate time remaining
            int timeRemainingSeconds = 0;
            if (quiz.TimeLimitMinutes > 0)
            {
                var elapsed = (DateTime.Now - attempt.StartedAt).TotalMinutes;
                var remaining = quiz.TimeLimitMinutes - elapsed;
                timeRemainingSeconds = (int)Math.Max(0, remaining * 60);
                
                if (timeRemainingSeconds == 0)
                {
                    timeRemainingSeconds = quiz.TimeLimitMinutes * 60;
                }
            }

            // Calculate remaining attempts
            var remainingAttempts = quiz.MaxAttempts > 0 ? quiz.MaxAttempts - attemptCount : -1;

            var model = new QuizTakeVM
            {
                Quiz = quiz,
                Attempt = attempt,
                Questions = questionVMs,
                SelectedAnswers = attempt.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedOptionId),
                TimeRemainingSeconds = timeRemainingSeconds,
                RemainingAttempts = remainingAttempts
            };

            return View(model);
        }

        // POST: Save Answer
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveAnswer([FromBody] SaveAnswerRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Unauthorized" });

            var attempt = await _quizService.GetAttemptByIdAsync(request.AttemptId);
            if (attempt == null || attempt.StudentId != user.Id || attempt.SubmittedAt != null)
            {
                return Json(new { success = false, message = "Không thể lưu câu trả lời" });
            }

            await _quizService.SaveAnswerAsync(request.AttemptId, request.QuestionId, request.OptionId);
            return Json(new { success = true });
        }

        // POST: Submit Quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int attemptId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var attempt = await _quizService.GetAttemptByIdAsync(attemptId);
            if (attempt == null || attempt.StudentId != user.Id || attempt.SubmittedAt != null)
            {
                TempData["ErrorMessage"] = "Không thể nộp bài";
                return RedirectToAction("Index");
            }

            await _quizService.SubmitQuizAttemptAsync(attemptId);
            TempData["SuccessMessage"] = "Đã nộp bài thành công!";
            return RedirectToAction("ViewResult", new { attemptId });
        }

        // GET: View Result
        public async Task<IActionResult> ViewResult(int attemptId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var attempt = await _quizService.GetAttemptByIdAsync(attemptId);
            if (attempt == null || attempt.StudentId != user.Id)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài làm";
                return RedirectToAction("Index");
            }

            if (!attempt.SubmittedAt.HasValue)
            {
                return RedirectToAction("Take", new { id = attempt.QuizId });
            }

            var quiz = await _quizService.GetQuizWithDetailsAsync(attempt.QuizId);
            if (quiz == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài kiểm tra";
                return RedirectToAction("Index");
            }

            var answers = attempt.Answers.ToList();
            var answerDetails = answers.Select(ans => new AnswerDetailVM
            {
                Question = quiz.Questions.First(q => q.Id == ans.QuestionId),
                SelectedOption = ans.SelectedOption,
                CorrectOption = quiz.Questions
                    .First(q => q.Id == ans.QuestionId)
                    .Options.FirstOrDefault(o => o.IsCorrect),
                IsCorrect = ans.IsCorrect,
                PointsEarned = ans.PointsEarned,
                ManualPoints = ans.ManualPoints,
                TeacherComment = ans.TeacherComment
            }).ToList();

            ViewBag.Quiz = quiz;
            ViewBag.Attempt = attempt;
            ViewBag.AnswerDetails = answerDetails;

            return View();
        }

        // GET: My Results
        public async Task<IActionResult> MyResults()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var attempts = await _quizService.GetAttemptsByStudentAsync(user.Id);
            ViewBag.Attempts = attempts;
            return View();
        }
    }

    public class SaveAnswerRequest
    {
        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }
    }
}

