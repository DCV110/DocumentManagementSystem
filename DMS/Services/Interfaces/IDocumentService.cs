using DMS.Models;
using DMS.ViewModels;

namespace DMS.Services.Interfaces
{
    public interface IDocumentService
    {
        // Upload & File Operations
        Task<Document> UploadDocumentAsync(DocumentUploadVM model, string userId, string webRootPath, bool replaceIfExists = false);
        Task<Document?> CheckDuplicateAsync(string fileName, int? folderId, string userId);
        Task<Document> ReplaceDocumentAsync(int existingDocumentId, DocumentUploadVM model, string userId, string webRootPath);
        Task<List<ZipEntryInfo>> GetZipContentsAsync(int documentId, string webRootPath);
        
        // Get Operations
        Task<Document?> GetDocumentByIdAsync(int id);
        Task<Document?> GetDocumentWithDetailsAsync(int id);
        Task<List<Document>> GetDocumentsByUserAsync(string userId);
        
        // Search & Filter
        Task<(List<Document> Documents, int TotalCount)> SearchDocumentsAsync(
            string? searchTerm,
            string? author,
            DateTime? fromDate,
            DateTime? toDate,
            string? fileType,
            int page = 1,
            int pageSize = 20);
        
        Task<(List<Document> Documents, int TotalCount)> GetDocumentsByLibraryAsync(
            string? search,
            int? courseId,
            string? sortBy,
            int page = 1,
            int pageSize = 5,
            string? studentId = null);
        
        // Download
        Task<(byte[] FileBytes, string ContentType, string FileName)?> DownloadDocumentAsync(int id, string webRootPath);
        Task<bool> IncrementViewCountAsync(int id);
        Task<bool> IncrementDownloadCountAsync(int id);
        
        // Approval Workflow
        Task<List<Document>> GetApprovalQueueAsync(int page = 1, int pageSize = 10);
        Task<List<Document>> GetDocumentsByStatusAsync(string filter, int page = 1, int pageSize = 10); // filter: "pending", "approved", "rejected"
        Task<bool> ApproveDocumentAsync(int id);
        Task<bool> RejectDocumentAsync(int id, string reason);
        
        // Delete (Soft Delete)
        Task<bool> DeleteDocumentAsync(int id, string deletedBy);
        
        // Update Document Metadata
        Task<bool> UpdateDocumentAsync(int id, string title, string? description, int? folderId, int? courseId = null);
        
        // Sharing
        Task<bool> RequestPublicSharingAsync(int id);  // Yêu cầu chia sẻ lên thư viện công khai
        Task<bool> ApprovePublicSharingAsync(int id);  // Phê duyệt chia sẻ lên thư viện
        Task<bool> RejectPublicSharingAsync(int id, string reason);  // Từ chối chia sẻ lên thư viện
        Task<bool> UnpublishDocumentAsync(int id);  // Bỏ public document
        Task<bool> PublishDocumentAsync(int id);  // Mở lại public document
        Task<string?> GenerateShareLinkAsync(int id, DateTime? expiryDate = null);  // Tạo share link
        Task<Document?> GetDocumentByShareTokenAsync(string token);  // Lấy document qua share token
        
        // Recycle Bin
        Task<List<Document>> GetDeletedDocumentsAsync(string userId);
        Task<bool> RestoreDocumentAsync(int id);
        Task<bool> DeletePermanentlyAsync(int id, string webRootPath);
        
        // Helper Methods
        Task<List<Course>> GetAllCoursesAsync();
        Task<List<Folder>> GetAllFoldersAsync();
        
        // Storage Management
        Task<long> GetStorageUsedAsync(string userId); // Returns bytes used
        Task<bool> CheckStorageLimitAsync(string userId, long additionalBytes); // Check if user can upload additional bytes
        long GetStorageLimit(string userId); // Returns storage limit in bytes (15GB for instructors)
    }
}
