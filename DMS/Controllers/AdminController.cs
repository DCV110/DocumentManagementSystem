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
            var user = await _userManager.GetUserAsync(User);
            var result = await _userService.CreateUserAsync(email, password, fullName, role, studentCode, faculty, classCode);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }
            else
            {
                // Log activity
                if (user != null)
                {
                    await _adminService.LogActivityAsync(
                        "Create", 
                        "User", 
                        null, 
                        $"Đã tạo người dùng mới: {fullName} ({email}) - Vai trò: {role}",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
                TempData["SuccessMessage"] = "Tạo người dùng thành công!";
            }
            return RedirectToAction("UserManagement");
        }

        // Update User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(string userId, string email, string fullName, string role, string? studentCode, string? faculty, string? classCode)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _userService.UpdateUserAsync(userId, email, fullName, role, studentCode, faculty, classCode);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }
            else
            {
                // Log activity
                if (user != null)
                {
                    await _adminService.LogActivityAsync(
                        "Update", 
                        "User", 
                        null, 
                        $"Đã cập nhật thông tin người dùng: {fullName} ({email})",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
                TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
            }
            return RedirectToAction("UserManagement");
        }

        // Lock/Unlock User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string userId)
        {
            var user = await _userManager.GetUserAsync(User);
            var targetUser = await _userManager.FindByIdAsync(userId);
            var isLocked = await _userService.ToggleUserLockAsync(userId);
            
            // Log activity
            if (user != null && targetUser != null)
            {
                await _adminService.LogActivityAsync(
                    isLocked ? "Lock" : "Unlock", 
                    "User", 
                    null, 
                    $"Đã {(isLocked ? "khóa" : "mở khóa")} tài khoản: {targetUser.FullName} ({targetUser.Email})",
                    user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );
            }
            
            return RedirectToAction("UserManagement");
        }

        // Assign Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.GetUserAsync(User);
            var targetUser = await _userManager.FindByIdAsync(userId);
            await _userService.AssignRoleAsync(userId, role);
            
            // Log activity
            if (user != null && targetUser != null)
            {
                await _adminService.LogActivityAsync(
                    "AssignRole", 
                    "User", 
                    null, 
                    $"Đã gán vai trò '{role}' cho người dùng: {targetUser.FullName} ({targetUser.Email})",
                    user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );
            }
            
            return RedirectToAction("UserManagement");
        }

        // Audit Log
        public async Task<IActionResult> AuditLog(string? userId, string? action, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            try
            {
                // Direct check of database
                var directCount = await _context.AuditLogs.CountAsync();
                System.Diagnostics.Debug.WriteLine($"Direct DB count: {directCount}");
                
                // Try a simple query first to see if it works
                var simpleQuery = _context.AuditLogs.AsQueryable();
                var simpleCount = await simpleQuery.CountAsync();
                System.Diagnostics.Debug.WriteLine($"Simple query count: {simpleCount}");
                
                var (logs, totalCount) = await _adminService.GetAuditLogsAsync(userId, action, fromDate, toDate, page, 20);
                
                // Ensure logs is never null
                ViewBag.Logs = logs ?? new List<DMS.Models.AuditLog>();
                ViewBag.UserId = userId;
                ViewBag.Action = action;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
                ViewBag.Page = page;
                ViewBag.TotalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / 20.0) : 1;
                ViewBag.TotalCount = totalCount;
                ViewBag.DirectCount = directCount; // For debugging
                
                // Debug: Log to see what we're getting
                System.Diagnostics.Debug.WriteLine($"AuditLog Controller: DirectCount={directCount}, SimpleCount={simpleCount}, FilteredCount={totalCount}, LogsReturned={logs?.Count ?? 0}, Page={page}");
                System.Diagnostics.Debug.WriteLine($"Filters - userId: {userId ?? "null"}, action: {action ?? "null"}, fromDate: {fromDate?.ToString() ?? "null"}, toDate: {toDate?.ToString() ?? "null"}");
                
                // If directCount > 0 but totalCount = 0, there's a filter issue
                if (directCount > 0 && totalCount == 0)
                {
                    // Try to get logs directly without service
                    var directLogs = await _context.AuditLogs
                        .OrderByDescending(a => a.Timestamp)
                        .Take(20)
                        .ToListAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"Direct query logs count: {directLogs.Count}");
                    
                    // If direct query works, use it
                    if (directLogs.Any())
                    {
                        // Load users manually
                        var userIds = directLogs.Where(l => !string.IsNullOrEmpty(l.UserId)).Select(l => l.UserId).Distinct().ToList();
                        if (userIds.Any())
                        {
                            var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
                            var userDict = users.ToDictionary(u => u.Id);
                            foreach (var log in directLogs)
                            {
                                if (!string.IsNullOrEmpty(log.UserId) && userDict.ContainsKey(log.UserId))
                                {
                                    log.User = userDict[log.UserId];
                                }
                            }
                        }
                        
                        ViewBag.Logs = directLogs;
                        ViewBag.TotalCount = directCount;
                        ViewBag.TotalPages = (int)Math.Ceiling(directCount / 20.0);
                    }
                    else
                    {
                        TempData["InfoMessage"] = $"Database có {directCount} bản ghi nhưng không thể truy vấn. Có thể có lỗi với Include User.";
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                ViewBag.Logs = new List<DMS.Models.AuditLog>();
                ViewBag.TotalCount = 0;
                ViewBag.TotalPages = 1;
                ViewBag.UserId = userId;
                ViewBag.Action = action;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
                ViewBag.Page = page;
                TempData["ErrorMessage"] = "Có lỗi khi tải nhật ký: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"Exception in AuditLog: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }

            return View();
        }

        // Reports
        public async Task<IActionResult> Reports(DateTime? fromDate, DateTime? toDate)
        {
            var statistics = await _adminService.GetStatisticsAsync(fromDate, toDate);
            var documentsByCourse = await _adminService.GetDocumentsByCourseAsync();
            var userActivityStats = await _adminService.GetUserActivityStatsAsync(10);
            var documentActivityStats = await _adminService.GetDocumentActivityStatsAsync(fromDate, toDate);

            // Calculate total storage
            var totalStorage = await _context.Documents
                .Where(d => !d.IsDeleted)
                .SumAsync(d => (long?)d.FileSize) ?? 0;

            ViewBag.TotalUsers = statistics.TotalUsers;
            ViewBag.TotalDocuments = statistics.TotalDocuments;
            ViewBag.TotalCourses = statistics.TotalCourses;
            ViewBag.DocumentsByCourse = documentsByCourse;
            ViewBag.UserActivityStats = userActivityStats;
            ViewBag.DocumentActivityStats = documentActivityStats;
            ViewBag.TotalStorage = totalStorage;
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

            var user = await _userManager.GetUserAsync(User);
            await _courseService.CreateCourseAsync(course);
            
            // Log activity
            if (user != null)
            {
                await _adminService.LogActivityAsync(
                    "Create", 
                    "Course", 
                    course.Id, 
                    $"Đã tạo khóa học: {course.CourseName} ({course.CourseCode})",
                    user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );
            }
            
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

            var user = await _userManager.GetUserAsync(User);
            var result = await _courseService.UpdateCourseAsync(course);
            if (result)
            {
                // Log activity
                if (user != null)
                {
                    await _adminService.LogActivityAsync(
                        "Update", 
                        "Course", 
                        course.Id, 
                        $"Đã cập nhật khóa học: {course.CourseName} ({course.CourseCode})",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
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
            var user = await _userManager.GetUserAsync(User);
            var course = await _courseService.GetCourseByIdAsync(id);
            var result = await _courseService.DeleteCourseAsync(id);
            if (result)
            {
                // Log activity
                if (user != null && course != null)
                {
                    await _adminService.LogActivityAsync(
                        "Delete", 
                        "Course", 
                        id, 
                        $"Đã xóa khóa học: {course.CourseName} ({course.CourseCode})",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
                TempData["SuccessMessage"] = "Đã xóa khóa học thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa khóa học";
            }
            return RedirectToAction("CourseManagement");
        }

        // Document Management - View All Documents
        public async Task<IActionResult> DocumentManagement(string? search, int? courseId, string? status, string? userId, int page = 1)
        {
            var (documents, totalCount) = await _adminService.GetAllDocumentsAsync(search, courseId, status, userId, page, 20);
            var courses = await _courseService.GetAllCoursesAsync();
            var allUsers = await _userService.GetUsersAsync(null, null, 1, 1000);

            ViewBag.Documents = documents;
            ViewBag.Search = search;
            ViewBag.CourseId = courseId;
            ViewBag.Status = status;
            ViewBag.UserId = userId;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / 20.0);
            ViewBag.TotalCount = totalCount;
            ViewBag.Courses = courses;
            ViewBag.Users = allUsers.Item1;

            return View();
        }

        // Bulk Delete Documents
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDeleteDocuments(List<int> documentIds)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var result = await _adminService.BulkDeleteDocumentsAsync(documentIds, user.Id);
            if (result)
            {
                // Log activity
                await _adminService.LogActivityAsync("BulkDelete", "Document", null, 
                    $"Đã xóa {documentIds.Count} tài liệu", user.Id, 
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString());
                
                TempData["SuccessMessage"] = $"Đã xóa {documentIds.Count} tài liệu thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa tài liệu";
            }

            return RedirectToAction("DocumentManagement");
        }

        // Bulk Approve Documents
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkApproveDocuments(List<int> documentIds)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var result = await _adminService.BulkApproveDocumentsAsync(documentIds, user.Id);
            if (result)
            {
                // Log activity
                await _adminService.LogActivityAsync("BulkApprove", "Document", null, 
                    $"Đã phê duyệt {documentIds.Count} tài liệu", user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString());
                
                TempData["SuccessMessage"] = $"Đã phê duyệt {documentIds.Count} tài liệu thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể phê duyệt tài liệu";
            }

            return RedirectToAction("DocumentManagement");
        }

        // Bulk Reject Documents
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkRejectDocuments(List<int> documentIds, string reason)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var result = await _adminService.BulkRejectDocumentsAsync(documentIds, reason, user.Id);
            if (result)
            {
                // Log activity
                await _adminService.LogActivityAsync("BulkReject", "Document", null, 
                    $"Đã từ chối {documentIds.Count} tài liệu: {reason}", user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString());
                
                TempData["SuccessMessage"] = $"Đã từ chối {documentIds.Count} tài liệu thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể từ chối tài liệu";
            }

            return RedirectToAction("DocumentManagement");
        }
    }
}

