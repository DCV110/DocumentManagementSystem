using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DMS.Models;
using DMS.Services.Interfaces;
using DMS.ViewModels;

namespace DMS.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class QuizController : Controller
    {
        private readonly IQuizService _quizService;
        private readonly ICourseService _courseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuizController(
            IQuizService quizService,
            ICourseService courseService,
            UserManager<ApplicationUser> userManager)
        {
            _quizService = quizService;
            _courseService = courseService;
            _userManager = userManager;
        }

        // GET: Quiz List
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            List<Quiz> quizzes;
            
            if (isAdmin)
            {
                // Admin can see all quizzes
                quizzes = await _quizService.GetAllQuizzesAsync();
            }
            else
            {
                // Instructor can only see their own quizzes
                quizzes = await _quizService.GetQuizzesByInstructorAsync(user.Id);
            }
            
            ViewBag.Quizzes = quizzes;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.CurrentUserId = user.Id;
            return View();
        }

        // GET: Create Quiz
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Courses = await GetCoursesForUserAsync(user);
            return View(new QuizCreateVM());
        }

        // POST: Create Quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuizCreateVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Debug: Log model state errors
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"ModelState Error - Key: {error.Key}, Error: {err.ErrorMessage}");
                    }
                }
                ViewBag.Courses = await GetCoursesForUserAsync(user);
                return View(model);
            }

            // Debug: Check if Questions are being received
            System.Diagnostics.Debug.WriteLine($"Questions count: {model.Questions?.Count ?? 0}");
            if (model.Questions != null)
            {
                for (int i = 0; i < model.Questions.Count; i++)
                {
                    var q = model.Questions[i];
                    System.Diagnostics.Debug.WriteLine($"Question {i}: Text='{q.QuestionText}', Options count={q.Options?.Count ?? 0}");
                    if (q.Options != null)
                    {
                        for (int j = 0; j < q.Options.Count; j++)
                        {
                            var opt = q.Options[j];
                            System.Diagnostics.Debug.WriteLine($"  Option {j}: Text='{opt.OptionText}', IsCorrect={opt.IsCorrect}");
                        }
                    }
                }
            }

            // Validate questions
            if (model.Questions == null || !model.Questions.Any())
            {
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một câu hỏi");
                ViewBag.Courses = await GetCoursesForUserAsync(user);
                return View(model);
            }

            foreach (var question in model.Questions)
            {
                if (question.Options == null || question.Options.Count < 2)
                {
                    ModelState.AddModelError("", "Mỗi câu hỏi phải có ít nhất 2 lựa chọn");
                    var courses = await _courseService.GetCoursesByInstructorAsync(user.Id);
                    ViewBag.Courses = courses;
                    return View(model);
                }

                // Debug: Check IsCorrect values
                System.Diagnostics.Debug.WriteLine($"Question '{question.QuestionText}' - Options IsCorrect values:");
                foreach (var opt in question.Options)
                {
                    System.Diagnostics.Debug.WriteLine($"  Option '{opt.OptionText}': IsCorrect = {opt.IsCorrect}");
                }

                var hasCorrect = question.Options.Any(o => o.IsCorrect);
                System.Diagnostics.Debug.WriteLine($"  Has correct answer: {hasCorrect}");

                if (!hasCorrect)
                {
                    ModelState.AddModelError("", $"Câu hỏi '{question.QuestionText}': Mỗi câu hỏi phải có ít nhất một đáp án đúng");
                    var courses = await _courseService.GetCoursesByInstructorAsync(user.Id);
                    ViewBag.Courses = courses;
                    return View(model);
                }
            }

            try
            {
                var quiz = await _quizService.CreateQuizAsync(model, user.Id);
                TempData["SuccessMessage"] = "Đã tạo bài kiểm tra thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi tạo bài kiểm tra: {ex.Message}");
                ViewBag.Courses = await GetCoursesForUserAsync(user);
                return View(model);
            }
        }

        // GET: Edit Quiz
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var quiz = await _quizService.GetQuizWithDetailsAsync(id);
            if (quiz == null || !await CanAccessQuizAsync(quiz, user))
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài kiểm tra hoặc bạn không có quyền chỉnh sửa";
                return RedirectToAction("Index");
            }

            ViewBag.Courses = await GetCoursesForUserAsync(user);

            // Convert to ViewModel
            var model = new QuizCreateVM
            {
                Title = quiz.Title,
                Description = quiz.Description,
                CourseId = quiz.CourseId,
                StartDate = quiz.StartDate,
                EndDate = quiz.EndDate,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                MaxAttempts = quiz.MaxAttempts,
                Questions = quiz.Questions.OrderBy(q => q.QuestionOrder).Select(q => new QuestionCreateVM
                {
                    QuestionText = q.QuestionText,
                    Points = q.Points,
                    Options = q.Options.OrderBy(o => o.OptionOrder).Select(o => new OptionCreateVM
                    {
                        OptionText = o.OptionText,
                        OptionLabel = o.OptionLabel,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            };

            // Check if quiz has attempts
            var attempts = await _quizService.GetAttemptsByQuizAsync(id);
            var hasAttempts = attempts != null && attempts.Any();
            var submittedAttemptsCount = attempts?.Count(a => a.SubmittedAt != null) ?? 0;

            ViewBag.QuizId = id;
            ViewBag.IsPublished = quiz.IsPublished;
            ViewBag.HasAttempts = hasAttempts;
            ViewBag.SubmittedAttemptsCount = submittedAttemptsCount;
            return View(model);
        }

        // POST: Edit Quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuizCreateVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var quiz = await _quizService.GetQuizByIdAsync(id);
            if (quiz == null || !await CanAccessQuizAsync(quiz, user))
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài kiểm tra hoặc bạn không có quyền chỉnh sửa";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                List<Course> courses;
                
                if (isAdmin)
                {
                    courses = await _courseService.GetAllCoursesAsync();
                }
                else
                {
                    courses = await _courseService.GetCoursesByInstructorAsync(user.Id);
                }
                
                ViewBag.Courses = courses;
                ViewBag.QuizId = id;
                return View(model);
            }

            // Validate questions
            if (model.Questions == null || !model.Questions.Any())
            {
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một câu hỏi");
                var courses = await _courseService.GetCoursesByInstructorAsync(user.Id);
                ViewBag.Courses = courses;
                ViewBag.QuizId = id;
                return View(model);
            }

            try
            {
                // Check if quiz has submitted attempts
                var attempts = await _quizService.GetAttemptsByQuizAsync(id);
                var hasSubmittedAttempts = attempts != null && attempts.Any(a => a.SubmittedAt != null);
                
                // Check if quiz was published before update
                bool wasPublished = quiz.IsPublished;
                
                await _quizService.UpdateQuizAsync(id, model);
                
                // If quiz was published before, republish it to update for students
                // Use CourseId from model (which was just updated)
                if (wasPublished)
                {
                    if (model.CourseId.HasValue)
                    {
                        await _quizService.PublishQuizAsync(id, model.CourseId);
                        if (hasSubmittedAttempts)
                        {
                            TempData["SuccessMessage"] = "Đã cập nhật và công bố lại bài kiểm tra thành công! Lưu ý: Bài kiểm tra đã có người làm, các thay đổi sẽ ảnh hưởng đến kết quả đã chấm.";
                        }
                        else
                        {
                            TempData["SuccessMessage"] = "Đã cập nhật và công bố lại bài kiểm tra thành công!";
                        }
                    }
                    else
                    {
                        TempData["InfoMessage"] = "Đã cập nhật bài kiểm tra thành công! Vui lòng chọn khóa học và công bố lại từ trang danh sách.";
                    }
                }
                else
                {
                    if (hasSubmittedAttempts)
                    {
                        TempData["InfoMessage"] = "Đã cập nhật bài kiểm tra thành công! Lưu ý: Bài kiểm tra đã có người làm, các thay đổi sẽ ảnh hưởng đến kết quả đã chấm.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Đã cập nhật bài kiểm tra thành công!";
                    }
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi cập nhật bài kiểm tra: {ex.Message}");
                var courses = await _courseService.GetCoursesByInstructorAsync(user.Id);
                ViewBag.Courses = courses;
                ViewBag.QuizId = id;
                return View(model);
            }
        }

        // POST: Delete Quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var quiz = await _quizService.GetQuizByIdAsync(id);
            if (quiz == null || !await CanAccessQuizAsync(quiz, user))
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài kiểm tra hoặc bạn không có quyền xóa";
                return RedirectToAction("Index");
            }

            await _quizService.DeleteQuizAsync(id);
            TempData["SuccessMessage"] = "Đã xóa bài kiểm tra thành công!";
            return RedirectToAction("Index");
        }

        // POST: Publish Quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id, int? courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var quiz = await _quizService.GetQuizByIdAsync(id);
            if (quiz == null || !await CanAccessQuizAsync(quiz, user))
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài kiểm tra hoặc bạn không có quyền";
                return RedirectToAction("Index");
            }

            var finalCourseId = courseId ?? quiz.CourseId;
            
            if (!finalCourseId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn khóa học trước khi công bố bài kiểm tra";
                return RedirectToAction("Edit", new { id });
            }

            await _quizService.PublishQuizAsync(id, finalCourseId);
            TempData["SuccessMessage"] = "Đã công bố bài kiểm tra thành công!";
            return RedirectToAction("Index");
        }

        // POST: Unpublish Quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unpublish(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var quiz = await _quizService.GetQuizByIdAsync(id);
            if (quiz == null || !await CanAccessQuizAsync(quiz, user))
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài kiểm tra hoặc bạn không có quyền";
                return RedirectToAction("Index");
            }

            await _quizService.UnpublishQuizAsync(id);
            TempData["SuccessMessage"] = "Đã bỏ công bố bài kiểm tra thành công!";
            return RedirectToAction("Index");
        }

        // GET: View Results
        public async Task<IActionResult> Results(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var quiz = await _quizService.GetQuizByIdAsync(id);
            if (quiz == null || !await CanAccessQuizAsync(quiz, user))
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài kiểm tra hoặc bạn không có quyền";
                return RedirectToAction("Index");
            }

            var attempts = await _quizService.GetAttemptsByQuizAsync(id);
            var quizWithDetails = await _quizService.GetQuizWithDetailsAsync(id);

            var model = new QuizResultsVM
            {
                Quiz = quizWithDetails!,
                Attempts = attempts.Select(a => new AttemptResultVM
                {
                    Attempt = a,
                    Student = a.Student!,
                    Answers = a.Answers.Select(ans => new AnswerDetailVM
                    {
                        Question = quizWithDetails!.Questions.First(q => q.Id == ans.QuestionId),
                        SelectedOption = ans.SelectedOption,
                        CorrectOption = quizWithDetails.Questions
                            .First(q => q.Id == ans.QuestionId)
                            .Options.FirstOrDefault(o => o.IsCorrect),
                        IsCorrect = ans.IsCorrect,
                        PointsEarned = ans.PointsEarned,
                        ManualPoints = ans.ManualPoints,
                        TeacherComment = ans.TeacherComment
                    }).ToList()
                }).ToList(),
                TotalStudents = attempts.Select(a => a.StudentId).Distinct().Count(),
                CompletedCount = attempts.Count,
                AverageScore = attempts.Any() ? attempts.Average(a => a.ScorePercentage ?? 0) : 0
            };

            return View(model);
        }

        // POST: Manual Grade Attempt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeAttempt(int attemptId, int? manualScore, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Unauthorized" });

            var attempt = await _quizService.GetAttemptByIdAsync(attemptId);
            if (attempt == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài làm" });
            }

            var quiz = await _quizService.GetQuizByIdAsync(attempt.QuizId);
            if (quiz == null || !await CanAccessQuizAsync(quiz, user))
            {
                return Json(new { success = false, message = "Bạn không có quyền chấm điểm" });
            }

            await _quizService.ManualGradeAttemptAsync(attemptId, manualScore, comment);
            return Json(new { success = true, message = "Đã chấm điểm thành công" });
        }

        // GET: Get Attempt Details
        [HttpGet]
        public async Task<IActionResult> GetAttemptDetails(int attemptId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
            {
                return Content("<div class='error-message'><span class='material-symbols-outlined'>error</span><p>Unauthorized</p></div>", "text/html");
            }

            var attempt = await _quizService.GetAttemptByIdAsync(attemptId);
            if (attempt == null)
            {
                return Content("<div class='error-message'><span class='material-symbols-outlined'>error</span><p>Không tìm thấy bài làm</p></div>", "text/html");
            }

            var quiz = await _quizService.GetQuizByIdAsync(attempt.QuizId);
            if (quiz == null || !await CanAccessQuizAsync(quiz, user))
            {
                return Content("<div class='error-message'><span class='material-symbols-outlined'>error</span><p>Bạn không có quyền xem bài làm này</p></div>", "text/html");
            }

            // Get quiz with questions and options
            var quizWithDetails = await _quizService.GetQuizWithDetailsAsync(attempt.QuizId);
            if (quizWithDetails == null)
            {
                return Content("<div class='error-message'><span class='material-symbols-outlined'>error</span><p>Không tìm thấy bài kiểm tra</p></div>", "text/html");
            }

            var model = new AttemptDetailsVM
            {
                Attempt = attempt,
                Quiz = quizWithDetails,
                Student = attempt.Student!
            };

            return PartialView("_AttemptDetails", model);
        }

        // Helper method to check if user can access quiz
        private async Task<bool> CanAccessQuizAsync(Quiz quiz, ApplicationUser user)
        {
            if (quiz == null) return false;
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            return isAdmin || quiz.CreatedBy == user.Id;
        }

        // Helper method to get courses based on user role
        private async Task<List<Course>> GetCoursesForUserAsync(ApplicationUser user)
        {
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                return await _courseService.GetAllCoursesAsync();
            }
            else
            {
                return await _courseService.GetCoursesByInstructorAsync(user.Id);
            }
        }
    }
}

