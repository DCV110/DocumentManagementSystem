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
        
        // Phase 1: Core Reports
        Task<DocumentStatusReport> GetDocumentStatusReportAsync();
        Task<List<DocumentActivityStat>> GetDocumentActivityByWeekAsync(DateTime? fromDate, DateTime? toDate);
        Task<List<DocumentActivityStat>> GetDocumentActivityByMonthAsync(DateTime? fromDate, DateTime? toDate);
        Task<PublicShareReport> GetPublicShareReportAsync();
        
        // Phase 2: Additional Reports
        Task<List<DocumentEngagementStat>> GetDocumentEngagementStatsAsync(int top = 10);
        Task<List<UserByFacultyStat>> GetUsersByFacultyStatsAsync();
        Task<StorageDetailReport> GetStorageDetailReportAsync();
        
        // Phase 3: Advanced Reports
        Task<QuizReport> GetQuizReportAsync();
        Task<AuditLogReport> GetAuditLogReportAsync(DateTime? fromDate, DateTime? toDate);
        
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
        public int ViewCount { get; set; }
        public int DownloadCount { get; set; }
    }

    // Phase 1: Core Reports
    public class DocumentStatusReport
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Draft { get; set; }
        public double ApprovalRate { get; set; }
        public double RejectionRate { get; set; }
    }

    public class PublicShareReport
    {
        public int Requested { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public double ApprovalRate { get; set; }
    }

    // Phase 2: Additional Reports
    public class DocumentEngagementStat
    {
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int DownloadCount { get; set; }
        public double ConversionRate { get; set; }
    }

    public class UserByFacultyStat
    {
        public string Faculty { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public int DocumentCount { get; set; }
        public long TotalStorage { get; set; }
    }

    public class StorageDetailReport
    {
        public long TotalStorage { get; set; }
        public List<StorageByCourse> ByCourse { get; set; } = new();
        public List<StorageByFaculty> ByFaculty { get; set; } = new();
        public List<StorageByFileType> ByFileType { get; set; } = new();
    }

    public class StorageByCourse
    {
        public string CourseName { get; set; } = string.Empty;
        public long Storage { get; set; }
        public double Percentage { get; set; }
    }

    public class StorageByFaculty
    {
        public string Faculty { get; set; } = string.Empty;
        public long Storage { get; set; }
        public double Percentage { get; set; }
    }

    public class StorageByFileType
    {
        public string FileType { get; set; } = string.Empty;
        public long Storage { get; set; }
        public double Percentage { get; set; }
    }

    // Phase 3: Advanced Reports
    public class QuizReport
    {
        public int TotalQuizzes { get; set; }
        public int PublishedQuizzes { get; set; }
        public int UnpublishedQuizzes { get; set; }
        public int TotalAttempts { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
        public List<QuizByCourse> ByCourse { get; set; } = new();
    }

    public class QuizByCourse
    {
        public string CourseName { get; set; } = string.Empty;
        public int QuizCount { get; set; }
        public int AttemptCount { get; set; }
        public double AverageScore { get; set; }
    }

    public class AuditLogReport
    {
        public int TotalLogs { get; set; }
        public Dictionary<string, int> ByAction { get; set; } = new();
        public Dictionary<string, int> ByEntityType { get; set; } = new();
        public List<TopUserActivity> TopUsers { get; set; } = new();
    }

    public class TopUserActivity
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int ActivityCount { get; set; }
    }
}

