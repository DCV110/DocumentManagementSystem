using DMS.Data;
using DMS.Models;
using DMS.Services.Interfaces;
using DMS.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.IO.Compression;

namespace DMS.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFolderService _folderService;

        public DocumentService(ApplicationDbContext context, IFolderService folderService)
        {
            _context = context;
            _folderService = folderService;
        }

        // Check for duplicate document
        public async Task<Document?> CheckDuplicateAsync(string fileName, int? folderId, string userId)
        {
            var existingQuery = _context.Documents
                .Where(d => !d.IsDeleted && d.UserId == userId && d.FileName == fileName);

            if (folderId.HasValue)
            {
                return await existingQuery
                    .FirstOrDefaultAsync(d => d.FolderId == folderId.Value);
            }
            else
            {
                return await existingQuery
                    .FirstOrDefaultAsync(d => d.FolderId == null);
            }
        }

        // Replace existing document
        public async Task<Document> ReplaceDocumentAsync(int existingDocumentId, DocumentUploadVM model, string userId, string webRootPath)
        {
            if (model.File == null || model.File.Length == 0)
            {
                throw new ArgumentException("File không được để trống");
            }

            // Get existing document
            var existingDocument = await _context.Documents
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == existingDocumentId && d.UserId == userId);

            if (existingDocument == null)
            {
                throw new ArgumentException("Không tìm thấy tài liệu cần thay thế");
            }

            // Delete old physical file
            if (!string.IsNullOrEmpty(existingDocument.FilePath))
            {
                var oldFilePath = Path.Combine(webRootPath, existingDocument.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                    catch
                    {
                        // Log error but continue
                    }
                }
            }

            // Create uploads directory if not exists
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "documents");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename for new file
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save new file to disk
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lưu file: {ex.Message}", ex);
            }

            // Update document record
            var title = string.IsNullOrWhiteSpace(model.Title) 
                ? Path.GetFileNameWithoutExtension(model.File.FileName) 
                : model.Title;

            existingDocument.Title = title;
            existingDocument.Description = model.Description;
            existingDocument.FileName = model.File.FileName;
            existingDocument.FilePath = $"/uploads/documents/{uniqueFileName}";
            existingDocument.FileSize = model.File.Length;
            existingDocument.ContentType = model.File.ContentType;
            existingDocument.UploadDate = DateTime.Now;
            existingDocument.ViewCount = 0; // Reset view count
            existingDocument.DownloadCount = 0; // Reset download count
            existingDocument.IsDeleted = false; // Ensure not deleted
            existingDocument.DeletedDate = null;
            existingDocument.DeletedBy = null;

            await _context.SaveChangesAsync();

            return existingDocument;
        }

        // Upload Document
        public async Task<Document> UploadDocumentAsync(DocumentUploadVM model, string userId, string webRootPath, bool replaceIfExists = false)
        {
            if (model.File == null || model.File.Length == 0)
            {
                throw new ArgumentException("File không được để trống");
            }

            // Check for duplicate
            var duplicate = await CheckDuplicateAsync(model.File.FileName, model.FolderId, userId);
            if (duplicate != null)
            {
                if (replaceIfExists)
                {
                    return await ReplaceDocumentAsync(duplicate.Id, model, userId, webRootPath);
                }
                else
                {
                    throw new ArgumentException($"Đã tồn tại file với tên '{model.File.FileName}'" + 
                        (model.FolderId.HasValue ? " trong thư mục này" : ""));
                }
            }

            // Create uploads directory if not exists
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "documents");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file to disk
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lưu file: {ex.Message}", ex);
            }

            // Create document record
            var title = string.IsNullOrWhiteSpace(model.Title) 
                ? Path.GetFileNameWithoutExtension(model.File.FileName) 
                : model.Title;

            var document = new Document
            {
                Title = title,
                Description = model.Description,
                FileName = model.File.FileName,
                FilePath = $"/uploads/documents/{uniqueFileName}",
                FileSize = model.File.Length,
                ContentType = model.File.ContentType,
                CourseId = model.CourseId,
                FolderId = model.FolderId,
                UserId = userId,
                UploadDate = DateTime.Now,
                Status = DocumentStatus.Approved // Auto-approve for instructors/admins
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return document;
        }

        // Get Document by ID
        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            return await _context.Documents.FindAsync(id);
        }

        // Get Document with details (Course, User)
        public async Task<Document?> GetDocumentWithDetailsAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Course)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        // Get Documents by User
        public async Task<List<Document>> GetDocumentsByUserAsync(string userId)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId && !d.IsDeleted)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        // Advanced Search
        public async Task<(List<Document> Documents, int TotalCount)> SearchDocumentsAsync(
            string? searchTerm,
            string? author,
            DateTime? fromDate,
            DateTime? toDate,
            string? fileType,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Documents
                .Include(d => d.Course)
                .Include(d => d.User)
                .AsQueryable();

            // Search by term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.Title.Contains(searchTerm) ||
                                        (d.Description != null && d.Description.Contains(searchTerm)));
            }

            // Filter by author
            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(d => d.User.FullName.Contains(author));
            }

            // Filter by date range
            if (fromDate.HasValue)
            {
                query = query.Where(d => d.UploadDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(d => d.UploadDate <= toDate.Value);
            }

            // Filter by file type
            if (!string.IsNullOrEmpty(fileType))
            {
                query = query.Where(d => d.ContentType.Contains(fileType));
            }

            var totalCount = await query.CountAsync();
            var documents = await query
                .OrderByDescending(d => d.UploadDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (documents, totalCount);
        }

        // Get Documents for Library (Student view)
        public async Task<(List<Document> Documents, int TotalCount)> GetDocumentsByLibraryAsync(
            string? search,
            int? courseId,
            string? sortBy,
            int page = 1,
            int pageSize = 5,
            string? studentId = null)
        {
            var query = _context.Documents
                .Include(d => d.Course)
                    .ThenInclude(c => c.Instructor)
                .Include(d => d.User)
                .Where(d => !d.IsDeleted && d.Status == DocumentStatus.Approved && 
                    (d.CourseId != null || (d.IsPublicShared && d.PublicShareApproved)))
                .AsQueryable();

            // Filter by student's enrolled courses if studentId is provided
            if (!string.IsNullOrEmpty(studentId))
            {
                // Get all courses that the student is enrolled in
                var enrolledCourseIds = await _context.StudentCourses
                    .Where(sc => sc.StudentId == studentId)
                    .Select(sc => sc.CourseId)
                    .ToListAsync();
                
                if (enrolledCourseIds.Any())
                {
                    // Only show documents from courses the student is enrolled in
                    query = query.Where(d => d.CourseId.HasValue && enrolledCourseIds.Contains(d.CourseId.Value));
                }
                else
                {
                    // Student not enrolled in any course, return empty
                    return (new List<Document>(), 0);
                }
            }

            // Filter by specific course if selected
            if (courseId.HasValue)
            {
                query = query.Where(d => d.CourseId == courseId.Value);
            }

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.Title.Contains(search) ||
                                        (d.Description != null && d.Description.Contains(search)));
            }

            // Sort
            query = sortBy switch
            {
                "oldest" => query.OrderBy(d => d.UploadDate),
                "name_asc" => query.OrderBy(d => d.Title),
                "name_desc" => query.OrderByDescending(d => d.Title),
                "downloads" => query.OrderByDescending(d => d.DownloadCount),
                "size" => query.OrderByDescending(d => d.FileSize),
                _ => query.OrderByDescending(d => d.UploadDate) // newest (default)
            };

            var totalCount = await query.CountAsync();
            var documents = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (documents, totalCount);
        }

        // Download Document
        public async Task<(byte[] FileBytes, string ContentType, string FileName)?> DownloadDocumentAsync(int id, string webRootPath)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null)
            {
                return null;
            }

            var filePath = Path.Combine(webRootPath, document.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return null;
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return (fileBytes, document.ContentType, document.FileName);
        }

        // Increment View Count
        public async Task<bool> IncrementViewCountAsync(int id)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null || document.IsDeleted)
            {
                return false;
            }

            document.ViewCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        // Increment Download Count
        public async Task<bool> IncrementDownloadCountAsync(int id)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null || document.IsDeleted)
            {
                return false;
            }

            document.DownloadCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        // Approval Queue
        public async Task<List<Document>> GetApprovalQueueAsync(int page = 1, int pageSize = 10)
        {
            // Get documents pending approval OR pending public share approval
            return await _context.Documents
                .Include(d => d.Course)
                .Include(d => d.User)
                .Where(d => !d.IsDeleted && 
                    (d.Status == DocumentStatus.Pending || 
                     (d.PublicShareRequested && !d.PublicShareApproved)))
                .OrderByDescending(d => d.UploadDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Document>> GetDocumentsByStatusAsync(string filter, int page = 1, int pageSize = 10)
        {
            var query = _context.Documents
                .Include(d => d.Course)
                .Include(d => d.User)
                .Where(d => !d.IsDeleted)
                .AsQueryable();

            if (filter == "pending")
            {
                // Documents pending approval OR pending public share approval
                query = query.Where(d => d.Status == DocumentStatus.Pending || 
                    (d.PublicShareRequested && !d.PublicShareApproved));
            }
            else if (filter == "approved")
            {
                // Only show documents that are approved for public sharing (not personal files)
                query = query.Where(d => d.IsPublicShared && d.PublicShareApproved);
            }
            else if (filter == "rejected")
            {
                // Documents that are rejected (public share requests that were rejected)
                query = query.Where(d => d.PublicShareRequested && !d.PublicShareApproved && 
                    !string.IsNullOrEmpty(d.RejectionReason));
            }

            return await query
                .OrderByDescending(d => d.UploadDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Approve Document
        public async Task<bool> ApproveDocumentAsync(int id)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null)
            {
                return false;
            }

            // TODO: Update Status field when it's added to Document model
            // document.Status = DocumentStatus.Approved;
            await _context.SaveChangesAsync();
            return true;
        }

        // Reject Document
        public async Task<bool> RejectDocumentAsync(int id, string reason)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null)
            {
                return false;
            }

            // TODO: Update Status field and RejectionReason when they're added to Document model
            // document.Status = DocumentStatus.Rejected;
            // document.RejectionReason = reason;
            await _context.SaveChangesAsync();
            return true;
        }

        // Delete Document (Soft Delete)
        public async Task<bool> DeleteDocumentAsync(int id, string deletedBy)
        {
            // Use IgnoreQueryFilters to get document even if it's soft-deleted
            var document = await _context.Documents
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (document == null || document.IsDeleted)
            {
                return false;
            }

            document.IsDeleted = true;
            document.DeletedDate = DateTime.Now;
            document.DeletedBy = deletedBy;
            
            await _context.SaveChangesAsync();
            return true;
        }

        // Update Document Metadata
        public async Task<bool> UpdateDocumentAsync(int id, string title, string? description, int? folderId, int? courseId = null)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null || document.IsDeleted)
            {
                return false;
            }

            document.Title = title;
            document.Description = description;
            document.FolderId = folderId;
            if (courseId.HasValue)
            {
                document.CourseId = courseId.Value;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

        // Request Public Sharing
        public async Task<bool> RequestPublicSharingAsync(int id)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null || document.IsDeleted)
            {
                return false;
            }

            document.PublicShareRequested = true;
            document.PublicShareApproved = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // Approve Public Sharing
        public async Task<bool> ApprovePublicSharingAsync(int id)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null || document.IsDeleted)
            {
                return false;
            }

            document.PublicShareRequested = false;
            document.PublicShareApproved = true;
            document.IsPublicShared = true;
            document.Status = DocumentStatus.Approved;
            await _context.SaveChangesAsync();
            return true;
        }

        // Reject Public Sharing
        public async Task<bool> RejectPublicSharingAsync(int id, string reason)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null || document.IsDeleted)
            {
                return false;
            }

            document.PublicShareRequested = false;
            document.PublicShareApproved = false;
            document.IsPublicShared = false;
            document.RejectionReason = reason;
            await _context.SaveChangesAsync();
            return true;
        }

        // Unpublish Document (Bỏ public)
        public async Task<bool> UnpublishDocumentAsync(int id)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null || document.IsDeleted)
            {
                return false;
            }

            document.IsPublicShared = false;
            document.PublicShareApproved = false;
            // Keep PublicShareToken in case they want to republish later
            await _context.SaveChangesAsync();
            return true;
        }

        // Publish Document (Mở lại public)
        public async Task<bool> PublishDocumentAsync(int id)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null || document.IsDeleted)
            {
                return false;
            }

            document.IsPublicShared = true;
            document.PublicShareApproved = true;
            // Generate token if not exists
            if (string.IsNullOrEmpty(document.PublicShareToken))
            {
                document.PublicShareToken = Guid.NewGuid().ToString("N");
            }
            await _context.SaveChangesAsync();
            return true;
        }

        // Generate Share Link
        public async Task<string?> GenerateShareLinkAsync(int id, DateTime? expiryDate = null)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null || document.IsDeleted)
            {
                return null;
            }

            // Generate a secure random token
            var token = Guid.NewGuid().ToString("N");

            document.PublicShareToken = token;
            // Note: We can add expiry logic later if needed
            await _context.SaveChangesAsync();

            return token;
        }

        // Get Document by Share Token
        public async Task<Document?> GetDocumentByShareTokenAsync(string token)
        {
            var document = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.PublicShareToken == token && !d.IsDeleted);

            if (document == null)
            {
                return null;
            }

            return document;
        }

        // Get Deleted Documents
        public async Task<List<Document>> GetDeletedDocumentsAsync(string userId)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId && d.IsDeleted)
                .OrderByDescending(d => d.DeletedDate)
                .ToListAsync();
        }

        // Restore Document
        public async Task<bool> RestoreDocumentAsync(int id)
        {
            var document = await _context.Documents
                .IgnoreQueryFilters() // Include soft-deleted documents
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (document == null || !document.IsDeleted)
            {
                return false;
            }

            document.IsDeleted = false;
            document.DeletedDate = null;
            document.DeletedBy = null;
            
            await _context.SaveChangesAsync();
            return true;
        }

        // Delete Document Permanently (Hard Delete)
        public async Task<bool> DeletePermanentlyAsync(int id, string webRootPath)
        {
            var document = await _context.Documents
                .IgnoreQueryFilters() // Include soft-deleted documents
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (document == null)
            {
                return false;
            }

            // Delete physical file
            if (!string.IsNullOrEmpty(document.FilePath))
            {
                var filePath = Path.Combine(webRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch
                    {
                        // Log error but continue with database deletion
                    }
                }
            }

            // Delete from database
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        // Helper: Get All Courses
        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        // Helper: Get All Folders
        public async Task<List<Folder>> GetAllFoldersAsync()
        {
            return await _context.Folders.ToListAsync();
        }

        // Get ZIP file contents (without extracting)
        public async Task<List<ZipEntryInfo>> GetZipContentsAsync(int documentId, string webRootPath)
        {
            var document = await GetDocumentByIdAsync(documentId);
            if (document == null)
            {
                throw new ArgumentException("Không tìm thấy tài liệu");
            }

            var fileExtension = Path.GetExtension(document.FileName).ToLower();
            if (fileExtension != ".zip")
            {
                throw new ArgumentException("File không phải là định dạng ZIP");
            }

            var zipFilePath = Path.Combine(webRootPath, document.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(zipFilePath))
            {
                throw new FileNotFoundException("Không tìm thấy file ZIP");
            }

            var entries = new List<ZipEntryInfo>();

            try
            {
                using (var archive = ZipFile.OpenRead(zipFilePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        entries.Add(new ZipEntryInfo
                        {
                            Name = string.IsNullOrEmpty(entry.Name) ? Path.GetFileName(entry.FullName.TrimEnd('/')) : entry.Name,
                            FullPath = entry.FullName,
                            IsDirectory = string.IsNullOrEmpty(entry.Name) && !string.IsNullOrEmpty(entry.FullName),
                            Size = entry.Length,
                            LastModified = entry.LastWriteTime.DateTime
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi đọc file ZIP: {ex.Message}", ex);
            }

            return entries;
        }

        // Helper method to get content type from file extension
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }
    }
}
