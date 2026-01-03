using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DMS.Models;
using DMS.Services.Interfaces;

namespace DMS.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class FolderController : Controller
    {
        private readonly IFolderService _folderService;
        private readonly ICourseService _courseService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAdminService _adminService;

        public FolderController(IFolderService folderService, ICourseService courseService, UserManager<ApplicationUser> userManager, IAdminService adminService)
        {
            _folderService = folderService;
            _courseService = courseService;
            _userManager = userManager;
            _adminService = adminService;
        }

        // GET: Folder - Redirect to MyDocuments (this page is no longer used)
        public IActionResult Index()
        {
            return RedirectToAction("MyDocuments", "Home");
        }

        // GET: Folder/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var courses = user != null ? await _courseService.GetCoursesByInstructorAsync(user.Id) : new List<Course>();
            ViewBag.Courses = courses;
            return View();
        }

        // POST: Folder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FolderName,CourseId")] Folder folder)
        {
            var user = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                await _folderService.CreateFolderAsync(folder);
                
                // Log activity
                if (user != null)
                {
                    await _adminService.LogActivityAsync(
                        "Create", 
                        "Folder", 
                        folder.Id, 
                        $"Đã tạo thư mục: {folder.FolderName}",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
                return RedirectToAction("MyDocuments", "Home");
            }

            var courses = user != null ? await _courseService.GetCoursesByInstructorAsync(user.Id) : new List<Course>();
            ViewBag.Courses = courses;
            return View(folder);
        }

        // GET: Folder/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var folder = await _folderService.GetFolderByIdAsync(id.Value);
            if (folder == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var courses = user != null ? await _courseService.GetCoursesByInstructorAsync(user.Id) : new List<Course>();
            ViewBag.Courses = courses;
            return View(folder);
        }

        // POST: Folder/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FolderName,CourseId,CreatedDate")] Folder folder)
        {
            if (id != folder.Id)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                var result = await _folderService.UpdateFolderAsync(folder);
                if (!result)
                {
                    return NotFound();
                }
                
                // Log activity
                if (user != null)
                {
                    await _adminService.LogActivityAsync(
                        "Update", 
                        "Folder", 
                        folder.Id, 
                        $"Đã cập nhật thư mục: {folder.FolderName}",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
                return RedirectToAction("MyDocuments", "Home");
            }

            var courses = user != null ? await _courseService.GetCoursesByInstructorAsync(user.Id) : new List<Course>();
            ViewBag.Courses = courses;
            return View(folder);
        }

        // POST: Folder/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var folder = await _folderService.GetFolderByIdAsync(id);
            var result = await _folderService.DeleteFolderAsync(id, user.Id);
            if (result)
            {
                // Log activity
                if (folder != null)
                {
                    await _adminService.LogActivityAsync(
                        "Delete", 
                        "Folder", 
                        id, 
                        $"Đã xóa thư mục: {folder.FolderName}",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
                TempData["SuccessMessage"] = "Thư mục đã được chuyển vào thùng rác.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa thư mục này.";
            }
            
            return RedirectToAction("MyDocuments", "Home");
        }

        // GET: Folder/RecycleBin
        [HttpGet]
        public async Task<IActionResult> RecycleBin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var deletedFolders = await _folderService.GetDeletedFoldersAsync(user.Id);
            ViewBag.DeletedFolders = deletedFolders;
            
            return View();
        }

        // POST: Folder/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var folder = await _folderService.GetFolderByIdAsync(id);
            var result = await _folderService.RestoreFolderAsync(id);
            if (result)
            {
                // Log activity
                if (user != null && folder != null)
                {
                    await _adminService.LogActivityAsync(
                        "Restore", 
                        "Folder", 
                        id, 
                        $"Đã khôi phục thư mục: {folder.FolderName}",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
                TempData["SuccessMessage"] = "Thư mục đã được khôi phục.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể khôi phục thư mục này.";
            }
            
            return RedirectToAction("RecycleBin", "Document");
        }

        // POST: Folder/DeletePermanently/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermanently(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if folder exists and is deleted
            var folder = await _folderService.GetFolderByIdAsync(id);
            if (folder == null || !folder.IsDeleted)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thư mục đã xóa";
                return RedirectToAction("RecycleBin", "Document");
            }

            // Check permission: user can only delete their own folders unless they're admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && folder.CreatedBy != user.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa vĩnh viễn thư mục này";
                return RedirectToAction("RecycleBin", "Document");
            }

            var result = await _folderService.DeletePermanentlyAsync(id);
            if (result)
            {
                // Log activity
                if (folder != null)
                {
                    await _adminService.LogActivityAsync(
                        "DeletePermanently", 
                        "Folder", 
                        id, 
                        $"Đã xóa vĩnh viễn thư mục: {folder.FolderName}",
                        user.Id,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }
                
                TempData["SuccessMessage"] = "Đã xóa vĩnh viễn thư mục thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa vĩnh viễn thư mục";
            }
            
            return RedirectToAction("RecycleBin", "Document");
        }

        // GET: Folder/Permissions/5
        public async Task<IActionResult> Permissions(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var folder = await _folderService.GetFolderByIdAsync(id.Value);
            if (folder == null)
            {
                return NotFound();
            }

            // Lấy danh sách users để phân quyền
            var users = await _userManager.Users.ToListAsync();
            ViewBag.Folder = folder;
            ViewBag.Users = users;

            return View();
        }

        // POST: Share Folder to Course
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(int folderId, int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var folder = await _folderService.GetFolderByIdAsync(folderId);
            if (folder == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thư mục";
                return RedirectToAction("MyDocuments", "Home");
            }

            // Check permission: user can only share their own folders unless they're admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && folder.CreatedBy != user.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chia sẻ thư mục này";
                return RedirectToAction("MyDocuments", "Home");
            }

            // Verify that the course belongs to this instructor (unless admin)
            if (!isAdmin)
            {
                var course = await _courseService.GetCourseByIdAsync(courseId);
                if (course == null || course.InstructorId != user.Id)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền chia sẻ vào khóa học này";
                    return RedirectToAction("MyDocuments", "Home");
                }
            }

            // Update folder's CourseId
            folder.CourseId = courseId;
            var result = await _folderService.UpdateFolderAsync(folder);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã chia sẻ thư mục thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể chia sẻ thư mục";
            }

            // Redirect back to previous page or MyDocuments
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && referer.Contains("/Home/CourseDetails"))
            {
                // Extract courseId from referer if possible
                try
                {
                    var uri = new Uri(referer);
                    var courseIdParam = Request.Query["id"].ToString();
                    if (int.TryParse(courseIdParam, out var courseIdFromReferer))
                    {
                        return RedirectToAction("CourseDetails", "Home", new { id = courseIdFromReferer });
                    }
                }
                catch
                {
                    // If parsing fails, just redirect to MyDocuments
                }
            }

            return RedirectToAction("MyDocuments", "Home");
        }
    }
}

