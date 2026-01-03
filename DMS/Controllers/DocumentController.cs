using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DMS.Models;
using DMS.Data;
using DMS.Services.Interfaces;
using DMS.ViewModels;

namespace DMS.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly IFolderService _folderService;
        private readonly ICourseService _courseService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly IAdminService _adminService;

        public DocumentController(
            IDocumentService documentService,
            IFolderService folderService,
            ICourseService courseService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ApplicationDbContext context,
            IAdminService adminService)
        {
            _documentService = documentService;
            _folderService = folderService;
            _courseService = courseService;
            _userManager = userManager;
            _environment = environment;
            _context = context;
            _adminService = adminService;
        }

        // Upload Document - Redirect to MyDocuments (upload is now integrated there)
        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public IActionResult Upload()
        {
            return RedirectToAction("MyDocuments", "Home");
        }

        // POST Upload is no longer needed - handled in HomeController.MyDocuments
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Obsolete("Upload is now handled in HomeController.MyDocuments")]
        public async Task<IActionResult> Upload(DocumentUploadVM model)
        {
            // Validate file
            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("File", "Vui lòng chọn file để tải lên");
            }
            else if (model.File.Length > 50 * 1024 * 1024) // 50MB
            {
                ModelState.AddModelError("File", "File quá lớn! Vui lòng chọn file nhỏ hơn 50MB");
            }

            if (ModelState.IsValid)
            {
                try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                    await _documentService.UploadDocumentAsync(model, user.Id, _environment.WebRootPath);

                    TempData["SuccessMessage"] = "Tải lên tài liệu thành công!";
                    return RedirectToAction("MyDocuments", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi tải lên: {ex.Message}");
                }
            }

            // Reload data for view
            var currentUser = await _userManager.GetUserAsync(User);
            var courses = currentUser != null ? await _courseService.GetCoursesByInstructorAsync(currentUser.Id) : new List<Course>();
            var folders = await _documentService.GetAllFoldersAsync();
            ViewBag.Courses = courses;
            ViewBag.Folders = folders;
            
            return View(model);
        }

        // Document Library - Student, Instructor, Admin
        [Authorize(Roles = "Student,Instructor,Admin")]
        public async Task<IActionResult> Library(string? search, int? courseId, string? sortBy, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            var isStudent = await _userManager.IsInRoleAsync(user, "Student");
            string? studentId = null;
            List<Course> courses;

            if (isStudent)
            {
                // For students: filter by enrolled courses
                studentId = user.Id;
                courses = await _context.StudentCourses
                    .Where(sc => sc.StudentId == user.Id)
                    .Include(sc => sc.Course)
                    .Select(sc => sc.Course)
                    .ToListAsync();
            }
            else
            {
                // For instructors and admins: show all courses
                courses = await _courseService.GetAllCoursesAsync();
            }
            
            // Get documents (filtered by student enrollment if student, otherwise all approved documents)
            var (documents, totalDocuments) = await _documentService.GetDocumentsByLibraryAsync(
                search, courseId, sortBy, page, 5, studentId);

            ViewBag.Search = search;
            ViewBag.CourseId = courseId;
            ViewBag.SortBy = sortBy;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalDocuments / 5.0);
            ViewBag.TotalDocuments = totalDocuments;
            ViewBag.Courses = courses ?? new List<Course>();
            ViewBag.Documents = documents;
            ViewBag.IsStudent = isStudent;

            return View();
        }

        // Search - All roles (có thể dùng cho cả Student)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(string? q, string? author, DateTime? fromDate, DateTime? toDate, string? fileType, int page = 1)
        {
            var (results, totalResults) = await _documentService.SearchDocumentsAsync(
                q, author, fromDate, toDate, fileType, page, 20);

            ViewBag.Query = q;
            ViewBag.Author = author;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.FileType = fileType;
            ViewBag.Page = page;
            ViewBag.TotalResults = totalResults;
            ViewBag.Results = results;

            return View();
        }

        // Preview - All roles
        [HttpGet]
        public async Task<IActionResult> Preview(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var document = await _documentService.GetDocumentWithDetailsAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            // Increment view count
            await _documentService.IncrementViewCountAsync(id);

            // Log activity
            if (user != null)
            {
                await _adminService.LogActivityAsync(
                    "View", 
                    "Document", 
                    id, 
                    $"Đã xem tài liệu: {document.Title}",
                    user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );
            }

            // Get ZIP contents if file is ZIP
            List<ZipEntryInfo>? zipContents = null;
            if (System.IO.Path.GetExtension(document.FileName).ToLower() == ".zip")
            {
                try
                {
                    zipContents = await _documentService.GetZipContentsAsync(id, _environment.WebRootPath);
                }
                catch
                {
                    // Ignore errors, just don't show ZIP contents
                }
            }

            ViewBag.Document = document;
            ViewBag.ZipContents = zipContents;
            return View();
        }

        // Download - All roles
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var document = await _documentService.GetDocumentByIdAsync(id);
            var result = await _documentService.DownloadDocumentAsync(id, _environment.WebRootPath);
            
            if (result == null)
            {
                return NotFound();
            }

            // Increment download count after successful download
            await _documentService.IncrementDownloadCountAsync(id);

            // Log activity
            if (user != null && document != null)
            {
                await _adminService.LogActivityAsync(
                    "Download", 
                    "Document", 
                    id, 
                    $"Đã tải xuống tài liệu: {document.Title}",
                    user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );
            }

            var (fileBytes, contentType, fileName) = result.Value;
            return File(fileBytes, contentType, fileName);
        }

        // Approval Queue - Admin & Instructor
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Approval(string filter = "pending", int page = 1)
        {
            var documents = await _documentService.GetDocumentsByStatusAsync(filter, page, 10);
            
            var totalDocuments = documents.Count;

            ViewBag.Documents = documents;
            ViewBag.Filter = filter;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalDocuments / 10.0);

            return View();
        }

        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var document = await _documentService.GetDocumentByIdAsync(id);
            
            await _documentService.ApproveDocumentAsync(id);
            
            // Log activity
            if (user != null && document != null)
            {
                await _adminService.LogActivityAsync(
                    "Approve", 
                    "Document", 
                    id, 
                    $"Đã phê duyệt tài liệu: {document.Title}",
                    user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );
            }
            
            return RedirectToAction("Approval");
        }

        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var user = await _userManager.GetUserAsync(User);
            var document = await _documentService.GetDocumentByIdAsync(id);
            
            await _documentService.RejectDocumentAsync(id, reason);
            
            // Log activity
            if (user != null && document != null)
            {
                await _adminService.LogActivityAsync(
                    "Reject", 
                    "Document", 
                    id, 
                    $"Đã từ chối tài liệu: {document.Title}. Lý do: {reason}",
                    user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );
            }
            
            return RedirectToAction("Approval");
        }

        // Delete Document - Admin & Instructor
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if document exists and belongs to user (or user is admin)
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài liệu";
                return RedirectToAction("MyDocuments", "Home");
            }

            // Check permission: user can only delete their own documents unless they're admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && document.UserId != user.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa tài liệu này";
                return RedirectToAction("MyDocuments", "Home");
            }

            var result = await _documentService.DeleteDocumentAsync(id, user.Id);
            if (result)
            {
                // Log activity
                if (document != null)
                {
                    await _adminService.LogActivityAsync(
                        "Delete", 
                        "Document", 
                        id, 
                        $"Đã xóa tài liệu: {document.Title}",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
                TempData["SuccessMessage"] = "Đã xóa tài liệu thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa tài liệu";
            }

            return RedirectToAction("MyDocuments", "Home");
        }

        // Recycle Bin - Admin & Instructor
        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public async Task<IActionResult> RecycleBin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var deletedDocuments = await _documentService.GetDeletedDocumentsAsync(user.Id);
            ViewBag.DeletedDocuments = deletedDocuments;

            // Also get deleted folders
            var deletedFolders = await _folderService.GetDeletedFoldersAsync(user.Id);
            ViewBag.DeletedFolders = deletedFolders;

            return View();
        }

        // Restore Document - Admin & Instructor
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            var result = await _documentService.RestoreDocumentAsync(id);
            if (result)
            {
                // Log activity
                if (document != null)
                {
                    await _adminService.LogActivityAsync(
                        "Restore", 
                        "Document", 
                        id, 
                        $"Đã khôi phục tài liệu: {document.Title}",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
                TempData["SuccessMessage"] = "Đã khôi phục tài liệu thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể khôi phục tài liệu";
            }

            return RedirectToAction("RecycleBin");
        }

        // Delete Permanently - Admin & Instructor
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermanently(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if document exists and is deleted
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null || !document.IsDeleted)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài liệu đã xóa";
                return RedirectToAction("RecycleBin");
            }

            // Check permission: user can only delete their own documents unless they're admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && document.UserId != user.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa vĩnh viễn tài liệu này";
                return RedirectToAction("RecycleBin");
            }

            var result = await _documentService.DeletePermanentlyAsync(id, _environment.WebRootPath);
            if (result)
            {
                // Log activity
                if (document != null)
                {
                    await _adminService.LogActivityAsync(
                        "DeletePermanently", 
                        "Document", 
                        id, 
                        $"Đã xóa vĩnh viễn tài liệu: {document.Title}",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
                TempData["SuccessMessage"] = "Đã xóa vĩnh viễn tài liệu thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa vĩnh viễn tài liệu";
            }

            return RedirectToAction("RecycleBin");
        }

        // Edit Document - Admin & Instructor
        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null || document.IsDeleted)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài liệu";
                return RedirectToReturnUrl(returnUrl) ?? RedirectToAction("MyDocuments", "Home");
            }

            // Check permission: user can only edit their own documents unless they're admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && document.UserId != user.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa tài liệu này";
                return RedirectToReturnUrl(returnUrl) ?? RedirectToAction("MyDocuments", "Home");
            }

            var folders = await _documentService.GetAllFoldersAsync();
            ViewBag.Document = document;
            ViewBag.Folders = folders;
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string title, string? description, int? folderId, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null || document.IsDeleted)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài liệu";
                return RedirectToReturnUrl(returnUrl) ?? RedirectToAction("MyDocuments", "Home");
            }

            // Check permission
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && document.UserId != user.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa tài liệu này";
                return RedirectToReturnUrl(returnUrl) ?? RedirectToAction("MyDocuments", "Home");
            }

            // Validate
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên tài liệu";
                var folders = await _documentService.GetAllFoldersAsync();
                ViewBag.Document = document;
                ViewBag.Folders = folders;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var result = await _documentService.UpdateDocumentAsync(id, title, description, folderId, null);
            if (result)
            {
                // Log activity
                if (document != null)
                {
                    await _adminService.LogActivityAsync(
                        "Update", 
                        "Document", 
                        id, 
                        $"Đã cập nhật tài liệu: {title}",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
                TempData["SuccessMessage"] = "Đã cập nhật tài liệu thành công";
                return RedirectToReturnUrl(returnUrl) ?? RedirectToAction("MyDocuments", "Home");
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể cập nhật tài liệu";
                var folders = await _documentService.GetAllFoldersAsync();
                ViewBag.Document = document;
                ViewBag.Folders = folders;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
        }

        // Helper method to redirect to returnUrl if valid
        private IActionResult? RedirectToReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return null;
        }

        // POST: Share Document to Course
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(int? documentId, int? folderId, int? courseId, string? shareType, DateTime? linkExpiry, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (documentId.HasValue)
            {
                // Share document
                var document = await _documentService.GetDocumentByIdAsync(documentId.Value);
                if (document == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài liệu";
                    return RedirectToAction("MyDocuments", "Home");
                }

                // Check permission: user can only share their own documents unless they're admin
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin && document.UserId != user.Id)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền chia sẻ tài liệu này";
                    return RedirectToAction("MyDocuments", "Home");
                }

                // Handle different share types
                if (shareType == "public")
                {
                    // Request public sharing (needs admin approval)
                    var result = await _documentService.RequestPublicSharingAsync(documentId.Value);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Đã gửi yêu cầu chia sẻ lên thư viện công khai. Vui lòng chờ admin phê duyệt.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không thể gửi yêu cầu chia sẻ";
                    }
                }
                else if (shareType == "link")
                {
                    // Generate share link
                    var token = await _documentService.GenerateShareLinkAsync(documentId.Value, linkExpiry);
                    if (token != null)
                    {
                        var shareUrl = Url.Action("Shared", "Document", new { token = token }, Request.Scheme);
                        TempData["ShareLink"] = shareUrl;
                        TempData["SuccessMessage"] = "Đã tạo link chia sẻ thành công!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không thể tạo link chia sẻ";
                    }
                }
                else if (courseId.HasValue)
                {
                    // Share with course (existing functionality)
                    // Verify that the course belongs to this instructor (unless admin)
                    if (!isAdmin)
                    {
                        var course = await _courseService.GetCourseByIdAsync(courseId.Value);
                        if (course == null || course.InstructorId != user.Id)
                        {
                            TempData["ErrorMessage"] = "Bạn không có quyền chia sẻ vào khóa học này";
                            return RedirectToAction("MyDocuments", "Home");
                        }
                    }

                    // Update document's CourseId
                    var result = await _documentService.UpdateDocumentAsync(documentId.Value, document.Title, document.Description, document.FolderId, courseId);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Đã chia sẻ tài liệu thành công";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không thể chia sẻ tài liệu";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn cách chia sẻ";
                }
            }
            else if (folderId.HasValue)
            {
                // Share folder - redirect to FolderController
                return RedirectToAction("Share", "Folder", new { folderId = folderId.Value, courseId = courseId });
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng chọn tài liệu hoặc thư mục để chia sẻ";
                return RedirectToAction("MyDocuments", "Home");
            }

            // Redirect back to previous page or MyDocuments
            // First check returnUrl if provided
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            // Check if document has a folder and redirect to that folder
            if (documentId.HasValue)
            {
                var sharedDocument = await _documentService.GetDocumentByIdAsync(documentId.Value);
                if (sharedDocument != null && sharedDocument.FolderId.HasValue)
                {
                    return RedirectToAction("MyDocuments", "Home", new { folderId = sharedDocument.FolderId });
                }
            }
            
            // Check referer for CourseDetails
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && referer.Contains("/Home/CourseDetails"))
            {
                // Try to extract courseId from referer
                try
                {
                    var uri = new Uri(referer);
                    var query = uri.Query;
                    if (query.Contains("id="))
                    {
                        var idParam = query.Split('&').FirstOrDefault(p => p.Contains("id="));
                        if (idParam != null && int.TryParse(idParam.Split('=')[1], out var courseIdFromReferer))
                        {
                            return RedirectToAction("CourseDetails", "Home", new { id = courseIdFromReferer });
                        }
                    }
                }
                catch
                {
                    // If parsing fails, just redirect to MyDocuments
                }
            }

            return RedirectToAction("MyDocuments", "Home");
        }

        // POST: Share Document Publicly (Requires Admin Approval)
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SharePublic(int? documentId, int? folderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (documentId.HasValue)
            {
                var document = await _documentService.GetDocumentByIdAsync(documentId.Value);
                if (document == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài liệu";
                    return RedirectToAction("MyDocuments", "Home");
                }

                // Check permission
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin && document.UserId != user.Id)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền chia sẻ tài liệu này";
                    return RedirectToAction("MyDocuments", "Home");
                }

                // If admin, approve immediately. Otherwise, request approval
                if (isAdmin)
                {
                    document.IsPublicShared = true;
                    document.PublicShareRequested = true;
                    document.PublicShareApproved = true;
                    document.PublicShareToken = Guid.NewGuid().ToString("N");
                    document.ApprovedBy = user.Id;
                    document.ApprovedDate = DateTime.Now;
                }
                else
                {
                    document.PublicShareRequested = true;
                    document.PublicShareApproved = false;
                    document.PublicShareToken = Guid.NewGuid().ToString("N");
                }

                await _context.SaveChangesAsync();

                if (isAdmin)
                {
                    TempData["SuccessMessage"] = "Đã chia sẻ tài liệu công khai thành công";
                }
                else
                {
                    TempData["SuccessMessage"] = "Đã gửi yêu cầu chia sẻ công khai. Vui lòng chờ admin phê duyệt.";
                }
            }
            else if (folderId.HasValue)
            {
                // Folder sharing - similar logic can be added later
                TempData["ErrorMessage"] = "Chia sẻ thư mục công khai chưa được hỗ trợ";
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng chọn tài liệu để chia sẻ";
            }

            return RedirectToAction("MyDocuments", "Home");
        }

        // GET: Get or Create Share Link
        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public async Task<IActionResult> GetShareLink(int documentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var document = await _documentService.GetDocumentByIdAsync(documentId);
            if (document == null)
            {
                return Json(new { success = false, message = "Document not found" });
            }

            // Check permission
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && document.UserId != user.Id)
            {
                return Json(new { success = false, message = "No permission" });
            }

            // Generate token if not exists
            if (string.IsNullOrEmpty(document.PublicShareToken))
            {
                document.PublicShareToken = Guid.NewGuid().ToString("N");
                await _context.SaveChangesAsync();
            }

            var shareLink = Url.Action("ViewByLink", "Document", new { token = document.PublicShareToken }, Request.Scheme);
            return Json(new { success = true, shareLink = shareLink });
        }

        // GET: View Document by Share Link
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ViewByLink(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.PublicShareToken == token && !d.IsDeleted);

            if (document == null)
            {
                return NotFound();
            }

            // Check if public sharing is enabled and approved
            if (!document.IsPublicShared && !document.PublicShareApproved)
            {
                return View("ShareLinkPending", document);
            }

            // Increment view count for shared link views
            await _documentService.IncrementViewCountAsync(document.Id);

            ViewBag.Document = document;
            return View("Preview", document);
        }

        // POST: Approve Public Share Request
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePublicShare(int documentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var document = await _documentService.GetDocumentByIdAsync(documentId);
            if (document == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài liệu";
                return RedirectToAction("Approval", "Document");
            }

            if (!document.PublicShareRequested)
            {
                TempData["ErrorMessage"] = "Tài liệu này chưa có yêu cầu chia sẻ công khai";
                return RedirectToAction("Approval", "Document");
            }

            document.IsPublicShared = true;
            document.PublicShareApproved = true;
            document.ApprovedBy = user.Id;
            document.ApprovedDate = DateTime.Now;

            // Also approve the document itself if it's pending
            if (document.Status == DocumentStatus.Pending)
            {
                document.Status = DocumentStatus.Approved;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã phê duyệt chia sẻ công khai thành công";

            return RedirectToAction("Approval", "Document");
        }

        // POST: Reject Public Share Request
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPublicShare(int documentId, string reason)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var document = await _documentService.GetDocumentByIdAsync(documentId);
            if (document == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài liệu";
                return RedirectToAction("Approval", "Document");
            }

            document.PublicShareRequested = false;
            document.PublicShareApproved = false;
            document.PublicShareToken = null;
            document.RejectionReason = reason;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã từ chối yêu cầu chia sẻ công khai";

            return RedirectToAction("Approval", "Document");
        }

        // POST: Unpublish Document (Bỏ public)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unpublish(int documentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _documentService.UnpublishDocumentAsync(documentId);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã bỏ chia sẻ công khai tài liệu thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể bỏ chia sẻ công khai tài liệu";
            }

            return RedirectToAction("Approval", "Document", new { filter = "approved" });
        }

        // POST: Publish Document (Mở lại public)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int documentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _documentService.PublishDocumentAsync(documentId);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã mở lại chia sẻ công khai tài liệu thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể mở lại chia sẻ công khai tài liệu";
            }

            return RedirectToAction("Approval", "Document", new { filter = "approved" });
        }
    }
}
