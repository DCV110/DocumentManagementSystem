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
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IDocumentService _documentService;
        private readonly ICourseService _courseService;
        private readonly IFolderService _folderService;
        private readonly IWebHostEnvironment _environment;
        private readonly IAdminService _adminService;

        public HomeController(
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context,
            IDocumentService documentService,
            ICourseService courseService,
            IFolderService folderService,
            IWebHostEnvironment environment,
            IAdminService adminService)
        {
            _userManager = userManager;
            _context = context;
            _documentService = documentService;
            _courseService = courseService;
            _folderService = folderService;
            _environment = environment;
            _adminService = adminService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(user);

            // Redirect đến dashboard tương ứng với role
            if (roles.Contains("Admin"))
                return RedirectToAction("AdminDashboard");
            else if (roles.Contains("Instructor"))
                return RedirectToAction("InstructorDashboard");
            else if (roles.Contains("Student"))
                return RedirectToAction("StudentDashboard");

            return RedirectToAction("Login", "Account");
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy thông tin cho dashboard sinh viên - chỉ lấy courses mà sinh viên đã đăng ký
            var enrolledCourses = await _context.StudentCourses
                .Where(sc => sc.StudentId == user.Id)
                .Include(sc => sc.Course)
                .Select(sc => sc.Course)
                .ToListAsync();
            var courses = enrolledCourses.Take(3).ToList();
            
            // Lấy danh sách course IDs mà sinh viên đã đăng ký
            var enrolledCourseIds = enrolledCourses.Select(c => c.Id).ToList();
            
            // Lấy tài liệu mới (trong 7 ngày qua) - từ các khóa học đã đăng ký (KHÔNG phải file public)
            var sevenDaysAgo = DateTime.Now.AddDays(-7);
            
            // Lấy file được chia sẻ vào các khóa học đã đăng ký (không phải file public)
            var courseDocuments = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Course)
                .Where(d => enrolledCourseIds.Contains(d.CourseId ?? 0) && 
                           !d.IsDeleted && 
                           !d.IsPublicShared &&
                           d.CourseId.HasValue)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
            
            // Lấy file public (từ thư viện)
            var (publicDocuments, _) = await _documentService.GetDocumentsByLibraryAsync(null, null, "newest", 1, 100, user.Id);
            
            // Kết hợp cả hai loại file
            var allDocuments = courseDocuments.Concat(publicDocuments)
                .OrderByDescending(d => d.UploadDate)
                .ToList();
            
            var newDocuments = allDocuments.Where(d => d.UploadDate >= sevenDaysAgo).ToList();
            
            // Lấy tài liệu đã tải (tạm thời lấy tất cả documents có thể truy cập)
            var downloadedCount = allDocuments.Count;
            
            // Lấy số thư mục có tài liệu mới
            var foldersWithNewDocs = allDocuments
                .Where(d => d.FolderId.HasValue && d.UploadDate >= sevenDaysAgo)
                .Select(d => d.FolderId!.Value)
                .Distinct()
                .Count();

            var documents = allDocuments.Take(4).ToList();
            
            // Tính số lượng tài liệu cho mỗi khóa học (chỉ file được chia sẻ vào khóa học, không phải file public)
            var courseDocumentCounts = new Dictionary<int, int>();
            foreach (var course in courses)
            {
                var count = courseDocuments.Count(d => d.CourseId == course.Id);
                courseDocumentCounts[course.Id] = count;
            }

            ViewBag.User = user;
            ViewBag.Courses = courses;
            ViewBag.CourseDocumentCounts = courseDocumentCounts; // Thêm dictionary để đếm đúng
            ViewBag.Documents = documents;
            ViewBag.NewDocumentsCount = newDocuments.Count;
            ViewBag.DownloadedCount = downloadedCount;
            ViewBag.FollowingFoldersCount = foldersWithNewDocs;
            ViewBag.CoursesCount = enrolledCourses.Count;

            return View();
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> InstructorDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy thông tin cho dashboard giảng viên
            var allCourses = await _courseService.GetCoursesByInstructorAsync(user.Id);
            var courses = allCourses.Take(3).ToList(); // Lấy 3 khóa học đầu tiên của giảng viên
            var allDocuments = await _documentService.GetDocumentsByUserAsync(user.Id);
            var allFolders = await _folderService.GetFoldersByUserAsync(user.Id);
            var documents = allDocuments.Take(4).ToList();

            // Tính số lượng documents và folders cho mỗi course
            var courseItemCounts = new Dictionary<int, int>();
            foreach (var course in courses)
            {
                var documentCount = allDocuments.Count(d => d.CourseId == course.Id && !d.IsDeleted);
                var folderCount = allFolders.Count(f => f.CourseId == course.Id && !f.IsDeleted);
                courseItemCounts[course.Id] = documentCount + folderCount;
            }

            ViewBag.User = user;
            ViewBag.Courses = courses;
            ViewBag.CourseItemCounts = courseItemCounts;
            ViewBag.Documents = documents;
            ViewBag.TotalDocuments = allDocuments.Count;
            ViewBag.TotalViews = allDocuments.Sum(d => d.ViewCount);
            ViewBag.TotalDownloads = allDocuments.Sum(d => d.DownloadCount);
            ViewBag.RejectedDocuments = allDocuments.Count(d => d.Status == DocumentStatus.Rejected);
            ViewBag.AllCourses = allCourses; // For share modal - chỉ courses của giảng viên này

            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy thông tin cho dashboard admin
            var totalUsers = await _userManager.Users.CountAsync();
            var totalDocuments = await _context.Documents.CountAsync(d => !d.IsDeleted);
            var pendingDocuments = await _context.Documents.CountAsync(d => !d.IsDeleted && d.Status == DocumentStatus.Pending);

            // Tính tăng trưởng tuần này
            var startOfWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
            var usersThisWeek = await _userManager.Users.CountAsync(u => u.CreatedDate >= startOfWeek);
            
            // Tính tăng trưởng tháng này
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var documentsThisMonth = await _context.Documents.CountAsync(d => !d.IsDeleted && d.UploadDate >= startOfMonth);

            // Tính số tài liệu đã duyệt hôm nay
            var today = DateTime.Today;
            var approvedToday = await _context.Documents.CountAsync(d => 
                !d.IsDeleted && 
                d.Status == DocumentStatus.Approved && 
                d.ApprovedDate.HasValue && 
                d.ApprovedDate.Value.Date == today);

            // Tính số tài liệu còn lại chờ duyệt (bao gồm cả public share requests)
            var urgentRequests = await _context.Documents.CountAsync(d => 
                !d.IsDeleted && 
                (d.Status == DocumentStatus.Pending || 
                 (d.PublicShareRequested && !d.PublicShareApproved)));

            // Tính storage usage
            var totalStorageBytes = await _context.Documents
                .Where(d => !d.IsDeleted)
                .SumAsync(d => (long?)d.FileSize) ?? 0;
            var totalStorageGB = totalStorageBytes / (1024.0 * 1024.0 * 1024.0);
            var storagePercentage = totalStorageGB > 0 ? Math.Min(100, (totalStorageGB / 2.0) * 100) : 0; // Giả sử 2TB total

            // Lấy hoạt động gần đây (Recent Activities)
            var recentDocuments = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Course)
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.UploadDate)
                .Take(5)
                .ToListAsync();

            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.CreatedDate)
                .Take(3)
                .ToListAsync();

            // Tạo danh sách hoạt động với DateTime để sắp xếp
            var activities = new List<(DateTime DateTime, string Type, string Title, string Description, ApplicationUser User, string Status, string StatusClass)>();
            
            foreach (var doc in recentDocuments)
            {
                if (doc.User != null) // Đảm bảo User được load
                {
                    activities.Add((
                        doc.UploadDate,
                        "document_upload",
                        "Nộp tài liệu mới",
                        doc.Title,
                        doc.User,
                        doc.Status == DocumentStatus.Pending ? "Chờ duyệt" : 
                        doc.Status == DocumentStatus.Approved ? "Đã phê duyệt" : "Đã từ chối",
                        doc.Status == DocumentStatus.Pending ? "blue" : 
                        doc.Status == DocumentStatus.Approved ? "gray" : "red"
                    ));
                }
            }

            foreach (var newUser in recentUsers)
            {
                activities.Add((
                    newUser.CreatedDate,
                    "user_registration",
                    "Đăng ký thành viên",
                    newUser.Faculty ?? "Hệ thống",
                    newUser,
                    "Hoàn tất",
                    "green"
                ));
            }

            // Sắp xếp và lấy 4 hoạt động gần nhất
            var recentActivities = activities
                .OrderByDescending(a => a.DateTime)
                .Take(4)
                .Select(a => new DMS.ViewModels.RecentActivityVM
                {
                    Type = a.Type,
                    Title = a.Title,
                    Description = a.Description,
                    User = a.User,
                    TimeAgo = GetTimeAgo(a.DateTime),
                    Status = a.Status,
                    StatusClass = a.StatusClass
                })
                .ToList();

            ViewBag.User = user;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalDocuments = totalDocuments;
            ViewBag.PendingDocuments = pendingDocuments;
            ViewBag.UsersThisWeek = usersThisWeek;
            ViewBag.DocumentsThisMonth = documentsThisMonth;
            ViewBag.ApprovedToday = approvedToday;
            ViewBag.UrgentRequests = urgentRequests;
            ViewBag.TotalStorageGB = totalStorageGB;
            ViewBag.StoragePercentage = storagePercentage;
            ViewBag.RecentActivities = recentActivities;

            return View();
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "Vừa xong";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            else if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} giờ trước";
            else if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} ngày trước";
            else if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} tuần trước";
            else
                return $"{(int)(timeSpan.TotalDays / 30)} tháng trước";
        }

        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public async Task<IActionResult> MyDocuments(string? search, string? sortBy, int? folderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Get current folder info if viewing a folder
            Folder? currentFolder = null;
            if (folderId.HasValue)
            {
                currentFolder = await _folderService.GetFolderWithDetailsAsync(folderId.Value);
                if (currentFolder == null || currentFolder.CreatedBy != user.Id)
                {
                    TempData["ErrorMessage"] = "Thư mục không tồn tại hoặc bạn không có quyền truy cập";
                    return RedirectToAction("MyDocuments");
                }
            }

            // Lấy tài liệu và thư mục của user hiện tại
            var allDocuments = await _documentService.GetDocumentsByUserAsync(user.Id);
            var allFolders = await _folderService.GetFoldersByUserAsync(user.Id);
            var courses = await _courseService.GetAllCoursesAsync();

            // Filter documents by folder if viewing a specific folder
            var documents = allDocuments.AsEnumerable();
            if (folderId.HasValue)
            {
                documents = documents.Where(d => d.FolderId == folderId.Value);
            }
            else
            {
                // Show only documents without folder (root level)
                documents = documents.Where(d => d.FolderId == null);
            }

            // Filter documents by search term
            if (!string.IsNullOrWhiteSpace(search))
            {
                documents = documents.Where(d => 
                    d.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (d.Description != null && d.Description.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (d.FileName != null && d.FileName.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            // Filter folders by folder (subfolders) if viewing a specific folder
            var folders = allFolders.AsEnumerable();
            if (folderId.HasValue)
            {
                // Show subfolders of current folder (exclude the current folder itself)
                folders = folders.Where(f => f.ParentFolderId == folderId.Value && f.Id != folderId.Value);
            }
            else
            {
                // Show only root folders (no parent)
                folders = folders.Where(f => f.ParentFolderId == null);
            }

            // Filter folders by search term
            if (!string.IsNullOrWhiteSpace(search))
            {
                folders = folders.Where(f => 
                    f.FolderName.Contains(search, StringComparison.OrdinalIgnoreCase)
                );
            }

            // Sort documents
            documents = sortBy switch
            {
                "oldest" => documents.OrderBy(d => d.UploadDate),
                "name_asc" => documents.OrderBy(d => d.Title),
                "name_desc" => documents.OrderByDescending(d => d.Title),
                "downloads" => documents.OrderByDescending(d => d.DownloadCount),
                "size" => documents.OrderByDescending(d => d.FileSize),
                _ => documents.OrderByDescending(d => d.UploadDate) // newest (default)
            };

            // Sort folders
            folders = sortBy switch
            {
                "oldest" => folders.OrderBy(f => f.CreatedDate),
                "name_asc" => folders.OrderBy(f => f.FolderName),
                "name_desc" => folders.OrderByDescending(f => f.FolderName),
                _ => folders.OrderByDescending(f => f.CreatedDate) // newest (default)
            };

            // Separate ZIP files from regular documents
            var zipFiles = documents.Where(d => d.FileName != null && 
                d.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).ToList();
            var documentsList = documents.Where(d => d.FileName == null || 
                !d.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).ToList();
            var foldersList = folders.ToList();

            // Calculate item counts for each folder (non-deleted documents + subfolders)
            var folderItemCounts = new Dictionary<int, int>();
            foreach (var folder in foldersList)
            {
                // Count non-deleted documents in this folder
                var documentCount = allDocuments.Count(d => d.FolderId == folder.Id && !d.IsDeleted);
                // Count subfolders of this folder
                var subfolderCount = allFolders.Count(f => f.ParentFolderId == folder.Id && !f.IsDeleted);
                // Store total count
                folderItemCounts[folder.Id] = documentCount + subfolderCount;
            }

            ViewBag.User = user;
            ViewBag.FolderItemCounts = folderItemCounts;
            ViewBag.Documents = documentsList;
            ViewBag.ZipFiles = zipFiles;
            ViewBag.AllDocuments = allDocuments; // For stats
            ViewBag.TotalViews = allDocuments.Sum(d => d.ViewCount);
            ViewBag.TotalDownloads = allDocuments.Sum(d => d.DownloadCount);
            ViewBag.ApprovedDocuments = allDocuments.Count(d => d.Status == DocumentStatus.Approved);
            ViewBag.RejectedDocuments = allDocuments.Count(d => d.Status == DocumentStatus.Rejected);
            ViewBag.TotalDocuments = allDocuments.Count;
            ViewBag.Folders = foldersList; // Filtered folders (based on search)
            ViewBag.AllFolders = allFolders; // All folders (not filtered) - for display even when searching
            ViewBag.TotalFolders = allFolders.Count;
            ViewBag.Courses = courses;
            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentFolderId = folderId;
            ViewBag.CurrentFolder = currentFolder; // For breadcrumb navigation

            return View();
        }

        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyDocuments(IFormFileCollection Files, string? Description, int? FolderId, string? replaceAction = null, string? duplicateFileNames = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Validate files
            if (Files == null || Files.Count == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một file để tải lên";
                if (FolderId.HasValue)
                {
                    return RedirectToAction("MyDocuments", new { folderId = FolderId });
                }
                return RedirectToAction("MyDocuments");
            }

            int successCount = 0;
            int failCount = 0;
            var errors = new List<string>();
            var duplicateFiles = new List<(string FileName, int DocumentId)>();

            // Parse duplicate file names if provided (format: "filename1:docId1,filename2:docId2")
            var duplicateMap = new Dictionary<string, int>();
            if (!string.IsNullOrEmpty(duplicateFileNames))
            {
                foreach (var item in duplicateFileNames.Split(','))
                {
                    var parts = item.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1], out var docId))
                    {
                        duplicateMap[parts[0]] = docId;
                    }
                }
            }

            // Upload each file
            foreach (var file in Files)
            {
                try
                {
                    // Validate file size
                    if (file.Length > 50 * 1024 * 1024) // 50MB
                    {
                        errors.Add($"File '{file.FileName}' quá lớn (tối đa 50MB)");
                        failCount++;
                        continue;
                    }

                    // Check for duplicate
                    var duplicate = await _documentService.CheckDuplicateAsync(file.FileName, FolderId, user.Id);
                    if (duplicate != null)
                    {
                        // Check if user wants to replace this specific file
                        if (replaceAction == "replace" && duplicateMap.ContainsKey(file.FileName))
                        {
                            // Replace existing document
                            var model = new DMS.ViewModels.DocumentUploadVM
                            {
                                Title = Path.GetFileNameWithoutExtension(file.FileName),
                                Description = Description,
                                FolderId = FolderId,
                                File = file
                            };
                            var replacedDoc = await _documentService.ReplaceDocumentAsync(duplicate.Id, model, user.Id, _environment.WebRootPath);
                            successCount++;
                            
                            // Log activity
                            if (replacedDoc != null)
                            {
                                await _adminService.LogActivityAsync(
                                    "Replace", 
                                    "Document", 
                                    replacedDoc.Id, 
                                    $"Đã thay thế tài liệu: {replacedDoc.Title}",
                                    user.Id,
                                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                                    Request.Headers["User-Agent"].ToString()
                                );
                            }
                        }
                        else
                        {
                            // Store duplicate info for client-side handling
                            duplicateFiles.Add((file.FileName, duplicate.Id));
                        }
                    }
                    else
                    {
                        // No duplicate, proceed with normal upload
                        var model = new DMS.ViewModels.DocumentUploadVM
                        {
                            Title = Path.GetFileNameWithoutExtension(file.FileName),
                            Description = Description,
                            FolderId = FolderId,
                            File = file
                        };

                        var uploadedDoc = await _documentService.UploadDocumentAsync(model, user.Id, _environment.WebRootPath, false);
                        successCount++;
                        
                        // Log activity
                        if (uploadedDoc != null)
                        {
                            await _adminService.LogActivityAsync(
                                "Upload", 
                                "Document", 
                                uploadedDoc.Id, 
                                $"Đã tải lên tài liệu: {uploadedDoc.Title}",
                                user.Id,
                                HttpContext.Connection.RemoteIpAddress?.ToString(),
                                Request.Headers["User-Agent"].ToString()
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Lỗi khi tải lên '{file.FileName}': {ex.Message}");
                    failCount++;
                }
            }

            // If there are duplicates and no replace action, return JSON for client-side handling
            if (duplicateFiles.Any() && replaceAction != "replace")
            {
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.ContentType?.Contains("multipart/form-data") == true)
                {
                    return Json(new
                    {
                        hasDuplicates = true,
                        duplicates = duplicateFiles.Select(d => new { fileName = d.FileName, documentId = d.DocumentId }).ToList(),
                        successCount = successCount,
                        failCount = failCount,
                        errors = errors
                    });
                }
                else
                {
                    // Regular form submission, store duplicates in TempData and redirect
                    TempData["DuplicateFiles"] = System.Text.Json.JsonSerializer.Serialize(
                        duplicateFiles.Select(d => new { fileName = d.FileName, documentId = d.DocumentId }).ToList()
                    );
                    TempData["PendingFiles"] = Files.Count;
                    if (successCount > 0)
                    {
                        TempData["SuccessMessage"] = $"Đã tải lên thành công {successCount} file";
                    }
                    return RedirectToAction("MyDocuments");
                }
            }

            // Set success/error messages
            if (successCount > 0 && failCount == 0)
            {
                TempData["SuccessMessage"] = $"Đã tải lên thành công {successCount} file!";
            }
            else if (successCount > 0 && failCount > 0)
            {
                TempData["SuccessMessage"] = $"Đã tải lên thành công {successCount} file";
                TempData["ErrorMessage"] = $"Không thể tải lên {failCount} file:\n" + string.Join("\n", errors);
            }
            else
            {
                TempData["ErrorMessage"] = $"Không thể tải lên file:\n" + string.Join("\n", errors);
            }

            // Redirect back to current folder if viewing one
            if (FolderId.HasValue)
            {
                return RedirectToAction("MyDocuments", new { folderId = FolderId });
            }
            return RedirectToAction("MyDocuments");
        }

        // Replace Document - AJAX endpoint
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplaceDocument(int documentId, IFormFile file, string? description, int? folderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Người dùng không hợp lệ" });

            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "File không được để trống" });
                }

                var model = new DMS.ViewModels.DocumentUploadVM
                {
                    Title = Path.GetFileNameWithoutExtension(file.FileName),
                    Description = description,
                    FolderId = folderId,
                    File = file
                };

                await _documentService.ReplaceDocumentAsync(documentId, model, user.Id, _environment.WebRootPath);
                return Json(new { success = true, message = "Đã thay thế file thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Create Folder - POST
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFolder(string folderName, int? courseId, int? parentFolderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(folderName))
            {
                TempData["ErrorMessage"] = "Tên thư mục không được để trống";
                if (parentFolderId.HasValue)
                {
                    return RedirectToAction("MyDocuments", new { folderId = parentFolderId });
                }
                return RedirectToAction("MyDocuments");
            }

            // Validate parent folder if provided
            if (parentFolderId.HasValue)
            {
                var parentFolder = await _folderService.GetFolderByIdAsync(parentFolderId.Value);
                if (parentFolder == null || parentFolder.CreatedBy != user.Id)
                {
                    TempData["ErrorMessage"] = "Thư mục cha không tồn tại hoặc bạn không có quyền truy cập";
                    return RedirectToAction("MyDocuments");
                }
            }

            var folder = new Folder
            {
                FolderName = folderName,
                CourseId = courseId, // null means no course (personal folder)
                ParentFolderId = parentFolderId, // null means root folder
                CreatedBy = user.Id,
                CreatedDate = DateTime.Now,
                IsPublic = false
            };

            try
            {
                await _folderService.CreateFolderAsync(folder);
                TempData["SuccessMessage"] = "Đã tạo thư mục thành công";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tạo thư mục: {ex.Message}";
            }

            // Redirect back to current folder if creating subfolder
            if (parentFolderId.HasValue)
            {
                return RedirectToAction("MyDocuments", new { folderId = parentFolderId });
            }
            return RedirectToAction("MyDocuments");
        }

        // My Courses - Student
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy danh sách khóa học mà sinh viên đã đăng ký
            var enrolledCourses = await _context.StudentCourses
                .Where(sc => sc.StudentId == user.Id)
                .Include(sc => sc.Course)
                .Select(sc => sc.Course)
                .ToListAsync();

            // Lấy danh sách course IDs
            var enrolledCourseIds = enrolledCourses.Select(c => c.Id).ToList();
            
            // Lấy file được chia sẻ vào các khóa học đã đăng ký (không phải file public)
            var courseDocuments = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Course)
                .Where(d => enrolledCourseIds.Contains(d.CourseId ?? 0) && 
                           !d.IsDeleted && 
                           !d.IsPublicShared &&
                           d.CourseId.HasValue)
                .ToListAsync();
            
            // Tính số lượng tài liệu cho mỗi khóa học (chỉ file được chia sẻ vào khóa học, không phải file public)
            var courseDocumentCounts = new Dictionary<int, int>();
            foreach (var course in enrolledCourses)
            {
                var count = courseDocuments.Count(d => d.CourseId == course.Id);
                courseDocumentCounts[course.Id] = count;
            }

            ViewBag.User = user;
            ViewBag.Courses = enrolledCourses;
            ViewBag.CourseDocumentCounts = courseDocumentCounts; // Thêm dictionary để đếm đúng
            ViewBag.CourseDocuments = courseDocuments; // Thêm để tính tài liệu mới trong view

            return View();
        }

        // Schedule - Student (Hoạt động tài liệu)
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Schedule()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy danh sách course IDs mà sinh viên đã đăng ký
            var enrolledCourseIds = await _context.StudentCourses
                .Where(sc => sc.StudentId == user.Id)
                .Select(sc => sc.CourseId)
                .ToListAsync();
            
            // Lấy file được chia sẻ vào các khóa học đã đăng ký (không phải file public)
            var courseDocuments = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Course)
                .Where(d => enrolledCourseIds.Contains(d.CourseId ?? 0) && 
                           !d.IsDeleted && 
                           !d.IsPublicShared &&
                           d.CourseId.HasValue)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
            
            // Lấy file public (từ thư viện)
            var (publicDocuments, _) = await _documentService.GetDocumentsByLibraryAsync(null, null, "newest", 1, 100);
            
            // Kết hợp cả hai loại file
            var allDocuments = courseDocuments.Concat(publicDocuments)
                .OrderByDescending(d => d.UploadDate)
                .ToList();
            
            var totalCount = allDocuments.Count;
            var recentDocuments = allDocuments.Take(10).ToList();

            // Tính toán thống kê
            var sevenDaysAgo = DateTime.Now.AddDays(-7);
            var documentsThisWeek = allDocuments.Where(d => d.UploadDate >= sevenDaysAgo).Count();
            var documentsThisMonth = allDocuments.Where(d => d.UploadDate.Month == DateTime.Now.Month && d.UploadDate.Year == DateTime.Now.Year).Count();
            
            // Nhóm theo khóa học - sử dụng Dictionary để dễ hiển thị trong view
            var documentsByCourseDict = allDocuments
                .GroupBy(d => d.Course?.CourseName ?? "Khác")
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.User = user;
            ViewBag.RecentDocuments = recentDocuments;
            ViewBag.DocumentsThisWeek = documentsThisWeek;
            ViewBag.DocumentsThisMonth = documentsThisMonth;
            ViewBag.TotalDocuments = totalCount;
            ViewBag.DocumentsByCourse = documentsByCourseDict;

            return View();
        }

        // GET: Course Details - View files and folders in a course
        [Authorize(Roles = "Admin,Instructor,Student")]
        [HttpGet]
        public async Task<IActionResult> CourseDetails(int id, string? search, string? sortBy)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Get course
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Check user role
            var isStudent = await _userManager.IsInRoleAsync(user, "Student");
            var isInstructor = await _userManager.IsInRoleAsync(user, "Instructor");
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            
            // If student, verify they are enrolled in this course
            if (isStudent)
            {
                var isEnrolled = await _context.StudentCourses
                    .AnyAsync(sc => sc.StudentId == user.Id && sc.CourseId == id);
                if (!isEnrolled)
                {
                    TempData["ErrorMessage"] = "Bạn chưa đăng ký khóa học này";
                    return RedirectToAction("MyCourses", "Home");
                }
            }

            // Get documents and folders for this course
            // For students: show ALL documents shared to this course (regardless of who uploaded)
            // For instructors/admins: show their own documents shared to this course
            List<Document> allDocuments;
            List<Folder> allFolders;
            IEnumerable<Document> documents;
            IEnumerable<Folder> folders;
            
            if (isStudent)
            {
                // Students see all documents shared to the course (but NOT public documents)
                // Public documents only appear in "Thư viện tài liệu" (Library page)
                allDocuments = await _context.Documents
                    .Include(d => d.User)
                    .Include(d => d.Course)
                    .Where(d => d.CourseId == id && !d.IsDeleted && !d.IsPublicShared)
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
                
                allFolders = await _context.Folders
                    .Include(f => f.Creator)
                    .Where(f => f.CourseId == id && !f.IsDeleted)
                    .OrderByDescending(f => f.CreatedDate)
                    .ToListAsync();
                
                // For students, documents and folders are already filtered, so no need to filter again
                documents = allDocuments.AsEnumerable();
                folders = allFolders.AsEnumerable();
            }
            else
            {
                // Instructors/Admins see only their own documents/folders shared to the course
                allDocuments = await _documentService.GetDocumentsByUserAsync(user.Id);
                allFolders = await _folderService.GetFoldersByUserAsync(user.Id);
                
                // Filter by course - Only show documents that are explicitly shared to this course (CourseId == id)
                // IMPORTANT: Exclude public documents (IsPublicShared) - they only appear in "Thư viện tài liệu" (Library page)
                // To show a document in this course, it must be explicitly shared to the course (CourseId must be set)
                // AND it must NOT be a public document (IsPublicShared = false)
                documents = allDocuments.Where(d => d.CourseId == id && !d.IsDeleted && !d.IsPublicShared).AsEnumerable();
                folders = allFolders.Where(f => f.CourseId == id && !f.IsDeleted).AsEnumerable();
            }

            // Apply search
            if (!string.IsNullOrWhiteSpace(search))
            {
                documents = documents.Where(d => 
                    d.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (d.Description != null && d.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
                folders = folders.Where(f => 
                    f.FolderName.Contains(search, StringComparison.OrdinalIgnoreCase)
                );
            }

            // Sort documents
            documents = sortBy switch
            {
                "oldest" => documents.OrderBy(d => d.UploadDate),
                "name_asc" => documents.OrderBy(d => d.Title),
                "name_desc" => documents.OrderByDescending(d => d.Title),
                "downloads" => documents.OrderByDescending(d => d.DownloadCount),
                "size" => documents.OrderByDescending(d => d.FileSize),
                _ => documents.OrderByDescending(d => d.UploadDate) // newest (default)
            };

            // Sort folders
            folders = sortBy switch
            {
                "oldest" => folders.OrderBy(f => f.CreatedDate),
                "name_asc" => folders.OrderBy(f => f.FolderName),
                "name_desc" => folders.OrderByDescending(f => f.FolderName),
                _ => folders.OrderByDescending(f => f.CreatedDate) // newest (default)
            };

            // Separate ZIP files from regular documents
            var zipFiles = documents.Where(d => d.FileName != null && 
                d.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).ToList();
            var documentsList = documents.Where(d => d.FileName == null || 
                !d.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).ToList();
            var foldersList = folders.ToList();

            // Calculate item counts for each folder
            var folderItemCounts = new Dictionary<int, int>();
            foreach (var folder in foldersList)
            {
                // For students: count all documents/folders in the folder
                // For instructors/admins: count only their own documents/folders
                int documentCount;
                int subfolderCount;
                
                if (isStudent)
                {
                    documentCount = await _context.Documents
                        .CountAsync(d => d.FolderId == folder.Id && !d.IsDeleted);
                    subfolderCount = await _context.Folders
                        .CountAsync(f => f.ParentFolderId == folder.Id && !f.IsDeleted);
                }
                else
                {
                    documentCount = allDocuments.Count(d => d.FolderId == folder.Id && !d.IsDeleted);
                    subfolderCount = allFolders.Count(f => f.ParentFolderId == folder.Id && !f.IsDeleted);
                }
                
                folderItemCounts[folder.Id] = documentCount + subfolderCount;
            }

            ViewBag.Course = course;
            ViewBag.Documents = documentsList;
            ViewBag.ZipFiles = zipFiles;
            ViewBag.Folders = foldersList;
            ViewBag.FolderItemCounts = folderItemCounts;
            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();

            return View();
        }
    }
}

