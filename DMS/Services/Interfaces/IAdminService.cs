namespace DMS.Services.Interfaces
{
    public interface IAdminService
    {
        // Statistics
        Task<AdminStatistics> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate);
        
        // Reports
        Task<List<DocumentsByCourseStat>> GetDocumentsByCourseAsync();
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
}

