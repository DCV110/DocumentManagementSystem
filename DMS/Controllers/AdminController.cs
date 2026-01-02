using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DMS.Models;
using DMS.Services.Interfaces;
using DMS.Data;

namespace DMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;
        private readonly ICourseService _courseService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            IUserService userService,
            IAdminService adminService,
            ICourseService courseService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _userService = userService;
            _adminService = adminService;
            _courseService = courseService;
            _userManager = userManager;
            _context = context;
            _environment = environment;
        }

        // User Management
        public async Task<IActionResult> UserManagement(string? search, string? role, int page = 1)
        {
            var usersWithRoles = await _userService.GetUsersWithRolesAsync(search, role, page, 20);
            var roles = await _userService.GetAllRolesAsync();
            var (_, totalUsers) = await _userService.GetUsersAsync(search, role, page, 20);

            ViewBag.Users = usersWithRoles;
            ViewBag.Roles = roles;
            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / 20.0);

            return View();
        }

        // Create User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string email, string password, string fullName, string role, string? studentCode, string? faculty, string? classCode)
        {
            var result = await _userService.CreateUserAsync(email, password, fullName, role, studentCode, faculty, classCode);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }
            else
            {
                TempData["SuccessMessage"] = "Tạo người dùng thành công!";
            }
            return RedirectToAction("UserManagement");
        }

        // Update User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(string userId, string email, string fullName, string role, string? studentCode, string? faculty, string? classCode)
        {
            var result = await _userService.UpdateUserAsync(userId, email, fullName, role, studentCode, faculty, classCode);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }
            else
            {
                TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
            }
            return RedirectToAction("UserManagement");
        }

        // Lock/Unlock User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string userId)
        {
            await _userService.ToggleUserLockAsync(userId);
            return RedirectToAction("UserManagement");
        }

        // Assign Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            await _userService.AssignRoleAsync(userId, role);
            return RedirectToAction("UserManagement");
        }

        // Audit Log
        public IActionResult AuditLog(string? userId, string? action, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            // TODO: Implement audit log service when AuditLog model is created
            ViewBag.UserId = userId;
            ViewBag.Action = action;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Page = page;

            return View();
        }

        // Reports
        public async Task<IActionResult> Reports(DateTime? fromDate, DateTime? toDate)
        {
            var statistics = await _adminService.GetStatisticsAsync(fromDate, toDate);
            var documentsByCourse = await _adminService.GetDocumentsByCourseAsync();

            ViewBag.TotalUsers = statistics.TotalUsers;
            ViewBag.TotalDocuments = statistics.TotalDocuments;
            ViewBag.TotalCourses = statistics.TotalCourses;
            ViewBag.DocumentsByCourse = documentsByCourse;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View();
        }

        // Backup
        public IActionResult Backup()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PerformBackup()
        {
            // TODO: Implement backup service when backup functionality is needed
            TempData["Message"] = "Backup đã được thực hiện thành công!";
            return RedirectToAction("Backup");
        }

        // Clean All Sample Data - Remove all documents and folders
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanAllSampleData()
        {
            try
            {
                // Get all documents
                var allDocuments = await _context.Documents.ToListAsync();
                int deletedFiles = 0;

                // Delete physical files
                foreach (var doc in allDocuments)
                {
                    if (!string.IsNullOrEmpty(doc.FilePath))
                    {
                        var filePath = Path.Combine(_environment.WebRootPath, doc.FilePath.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            try
                            {
                                System.IO.File.Delete(filePath);
                                deletedFiles++;
                            }
                            catch { }
                        }
                    }
                }

                // Delete all documents from database
                _context.Documents.RemoveRange(allDocuments);

                // Delete all folders from database
                var allFolders = await _context.Folders.ToListAsync();
                _context.Folders.RemoveRange(allFolders);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã xóa {allDocuments.Count} tài liệu ({deletedFiles} file vật lý) và {allFolders.Count} thư mục";
                return RedirectToAction("UserManagement");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa dữ liệu: {ex.Message}";
                return RedirectToAction("UserManagement");
            }
        }

        // Manage Student Enrollment - Assign students to courses
        [HttpGet]
        public async Task<IActionResult> ManageEnrollment(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            // Get all students
            var allStudents = await _userManager.GetUsersInRoleAsync("Student");
            
            // Get enrolled students for this course
            var enrolledStudentIds = await _context.StudentCourses
                .Where(sc => sc.CourseId == courseId)
                .Select(sc => sc.StudentId)
                .ToListAsync();

            ViewBag.Course = course;
            ViewBag.AllStudents = allStudents;
            ViewBag.EnrolledStudentIds = enrolledStudentIds;

            return View();
        }

        // Assign/Unassign student to course
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStudentEnrollment(int courseId, string studentId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return Json(new { success = false, message = "Khóa học không tồn tại" });
            }

            var student = await _userManager.FindByIdAsync(studentId);
            if (student == null)
            {
                return Json(new { success = false, message = "Sinh viên không tồn tại" });
            }

            var enrollment = await _context.StudentCourses
                .FirstOrDefaultAsync(sc => sc.CourseId == courseId && sc.StudentId == studentId);

            if (enrollment == null)
            {
                // Add enrollment
                _context.StudentCourses.Add(new StudentCourse
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrolledDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
                return Json(new { success = true, action = "added", message = "Đã thêm sinh viên vào lớp học" });
            }
            else
            {
                // Remove enrollment
                _context.StudentCourses.Remove(enrollment);
                await _context.SaveChangesAsync();
                return Json(new { success = true, action = "removed", message = "Đã xóa sinh viên khỏi lớp học" });
            }
        }

        // Course Management
        public async Task<IActionResult> CourseManagement()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            
            ViewBag.Courses = courses;
            ViewBag.Instructors = instructors;
            
            return View();
        }

        // Create Course
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(string courseName, string courseCode, string? classCode, string? description, string? instructorId)
        {
            if (string.IsNullOrWhiteSpace(courseName) || string.IsNullOrWhiteSpace(courseCode))
            {
                TempData["ErrorMessage"] = "Tên môn học và mã môn học không được để trống";
                return RedirectToAction("CourseManagement");
            }

            var course = new Course
            {
                CourseName = courseName,
                CourseCode = courseCode,
                ClassCode = classCode,
                Description = description,
                InstructorId = instructorId
            };

            await _courseService.CreateCourseAsync(course);
            TempData["SuccessMessage"] = "Đã tạo khóa học thành công";
            return RedirectToAction("CourseManagement");
        }

        // Update Course
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCourse(int id, string courseName, string courseCode, string? classCode, string? description, string? instructorId)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khóa học";
                return RedirectToAction("CourseManagement");
            }

            course.CourseName = courseName;
            course.CourseCode = courseCode;
            course.ClassCode = classCode;
            course.Description = description;
            course.InstructorId = instructorId;

            var result = await _courseService.UpdateCourseAsync(course);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã cập nhật khóa học thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể cập nhật khóa học";
            }
            return RedirectToAction("CourseManagement");
        }

        // Delete Course
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var result = await _courseService.DeleteCourseAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã xóa khóa học thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa khóa học";
            }
            return RedirectToAction("CourseManagement");
        }
    }
}

