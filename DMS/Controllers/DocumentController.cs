using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DMS.Data;
using DMS.Models;
using DMS.ViewModels;

namespace DMS.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public DocumentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // Upload Document - Admin & Instructor
        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var courses = await _context.Courses.ToListAsync();
            var folders = await _context.Folders.ToListAsync();
            
            ViewBag.Courses = courses;
            ViewBag.Folders = folders;
            
            return View();
        }

        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(DocumentUploadVM model)
        {
            if (ModelState.IsValid && model.File != null && model.File.Length > 0)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                // Lưu file
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(fileStream);
                }

                // Tạo document record
                var document = new Document
                {
                    Title = model.Title,
                    Description = model.Description,
                    FileName = model.File.FileName,
                    FilePath = $"/uploads/documents/{uniqueFileName}",
                    FileSize = model.File.Length,
                    ContentType = model.File.ContentType,
                    CourseId = model.CourseId,
                    FolderId = model.FolderId,
                    UserId = user.Id,
                    UploadDate = DateTime.Now
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                return RedirectToAction("MyDocuments", "Home");
            }

            var courses = await _context.Courses.ToListAsync();
            var folders = await _context.Folders.ToListAsync();
            ViewBag.Courses = courses;
            ViewBag.Folders = folders;
            
            return View(model);
        }

        // Document Library - Student
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Library(string? search, int? courseId, string? sortBy, int page = 1)
        {
            var query = _context.Documents
                .Include(d => d.Course)
                .Include(d => d.User)
                .Where(d => d.CourseId > 0); // Chỉ lấy documents đã được gán course

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.Title.Contains(search) || 
                                        (d.Description != null && d.Description.Contains(search)));
            }

            // Filter by course
            if (courseId.HasValue)
            {
                query = query.Where(d => d.CourseId == courseId.Value);
            }

            // Sort
            query = sortBy switch
            {
                "newest" => query.OrderByDescending(d => d.UploadDate),
                "oldest" => query.OrderBy(d => d.UploadDate),
                "name" => query.OrderBy(d => d.Title),
                _ => query.OrderByDescending(d => d.UploadDate)
            };

            var totalDocuments = await query.CountAsync();
            var pageSize = 5; // Changed to match UI design
            var documents = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var courses = await _context.Courses.ToListAsync();

            ViewBag.Search = search;
            ViewBag.CourseId = courseId;
            ViewBag.SortBy = sortBy;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalDocuments / (double)pageSize);
            ViewBag.TotalDocuments = totalDocuments;
            ViewBag.Courses = courses;
            ViewBag.Documents = documents;

            return View();
        }

        // Search - All roles (có thể dùng cho cả Student)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(string? q, string? author, DateTime? fromDate, DateTime? toDate, string? fileType, int page = 1)
        {
            var query = _context.Documents
                .Include(d => d.Course)
                .Include(d => d.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(d => d.Title.Contains(q) || 
                                        (d.Description != null && d.Description.Contains(q)));
            }

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(d => d.User.FullName.Contains(author));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(d => d.UploadDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(d => d.UploadDate <= toDate.Value);
            }

            if (!string.IsNullOrEmpty(fileType))
            {
                query = query.Where(d => d.ContentType.Contains(fileType));
            }

            var totalResults = await query.CountAsync();
            var results = await query
                .OrderByDescending(d => d.UploadDate)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();

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
            var document = await _context.Documents
                .Include(d => d.Course)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            ViewBag.Document = document;
            return View();
        }

        // Download - All roles
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, document.ContentType, document.FileName);
        }

        // Approval Queue - Admin & Instructor
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Approval(int page = 1)
        {
            // Giả sử có trường Status trong Document model
            // Tạm thời lấy tất cả documents, sau này sẽ filter theo status
            var documents = await _context.Documents
                .Include(d => d.Course)
                .Include(d => d.User)
                .OrderByDescending(d => d.UploadDate)
                .Skip((page - 1) * 10)
                .Take(10)
                .ToListAsync();

            var totalDocuments = await _context.Documents.CountAsync();

            ViewBag.Documents = documents;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalDocuments / 10.0);

            return View();
        }

        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                // Cập nhật status - cần thêm field Status vào Document model
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Approval");
        }

        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                // Cập nhật status - cần thêm field Status vào Document model
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Approval");
        }
    }
}
