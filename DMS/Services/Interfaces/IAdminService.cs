using DMS.Models;

namespace DMS.Services.Interfaces
{
    public interface IAdminService
    {
        // Statistics
        Task<AdminStatistics> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate);
        
        // Reports
        Task<List<DocumentsByCourseStat>> GetDocumentsByCourseAsync();
        Task<List<UserActivityStat>> GetUserActivityStatsAsync(int top = 10);
        Task<List<DocumentActivityStat>> GetDocumentActivityStatsAsync(DateTime? fromDate, DateTime? toDate);
        
        // Audit Log
        Task LogActivityAsync(string action, string? entityType, int? entityId, string? description, string userId, string? ipAddress = null, string? userAgent = null);
        Task<(List<AuditLog> Logs, int TotalCount)> GetAuditLogsAsync(string? userId, string? action, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20);
        
        // Document Management
        Task<(List<Document> Documents, int TotalCount)> GetAllDocumentsAsync(string? search, int? courseId, string? status, string? userId, int page = 1, int pageSize = 20);
        Task<bool> BulkDeleteDocumentsAsync(List<int> documentIds, string deletedBy);
        Task<bool> BulkApproveDocumentsAsync(List<int> documentIds, string approvedBy);
        Task<bool> BulkRejectDocumentsAsync(List<int> documentIds, string reason, string rejectedBy);
    }

    public class AdminStatistics
    {
        public int TotalUsers { get; set; }
        public int TotalDocuments { get; set; }
        public int TotalCourses { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class DocumentsByCourseStat
    {
        public string CourseName { get; set; } = string.Empty;
        public int Count { get; set; }
        public long TotalSize { get; set; }
    }

    public class UserActivityStat
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
        public long TotalStorage { get; set; }
        public int TotalViews { get; set; }
        public int TotalDownloads { get; set; }
    }

    public class DocumentActivityStat
    {
        public DateTime Date { get; set; }
        public int UploadCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }
}

