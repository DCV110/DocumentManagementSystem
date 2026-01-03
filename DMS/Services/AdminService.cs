using DMS.Data;
using DMS.Models;
using DMS.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DMS.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Get Statistics
        public async Task<AdminStatistics> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalDocuments = await _context.Documents.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();

            return new AdminStatistics
            {
                TotalUsers = totalUsers,
                TotalDocuments = totalDocuments,
                TotalCourses = totalCourses,
                FromDate = fromDate,
                ToDate = toDate
            };
        }

        // Get Documents by Course Statistics
        public async Task<List<DocumentsByCourseStat>> GetDocumentsByCourseAsync()
        {
            var stats = await _context.Documents
                .Include(d => d.Course)
                .GroupBy(d => d.Course != null ? d.Course.CourseName : "Unknown")
                .Select(g => new DocumentsByCourseStat
                {
                    CourseName = g.Key ?? "Unknown",
                    Count = g.Count(),
                    TotalSize = g.Sum(d => d.FileSize)
                })
                .ToListAsync();

            return stats;
        }

        // Get User Activity Statistics
        public async Task<List<UserActivityStat>> GetUserActivityStatsAsync(int top = 10)
        {
            var stats = await _context.Documents
                .Include(d => d.User)
                .Where(d => !d.IsDeleted)
                .GroupBy(d => new { d.UserId, d.User.FullName, d.User.Email })
                .Select(g => new UserActivityStat
                {
                    UserName = g.Key.FullName ?? "Unknown",
                    UserEmail = g.Key.Email ?? "",
                    DocumentCount = g.Count(),
                    TotalStorage = g.Sum(d => d.FileSize),
                    TotalViews = g.Sum(d => d.ViewCount),
                    TotalDownloads = g.Sum(d => d.DownloadCount)
                })
                .OrderByDescending(s => s.DocumentCount)
                .Take(top)
                .ToListAsync();

            return stats;
        }

        // Get Document Activity Statistics by Date
        public async Task<List<DocumentActivityStat>> GetDocumentActivityStatsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var startDate = fromDate ?? DateTime.Now.AddDays(-30);
            var endDate = toDate ?? DateTime.Now;

            var stats = await _context.Documents
                .Where(d => !d.IsDeleted && d.UploadDate >= startDate && d.UploadDate <= endDate)
                .GroupBy(d => d.UploadDate.Date)
                .Select(g => new DocumentActivityStat
                {
                    Date = g.Key,
                    UploadCount = g.Count(),
                    ApprovedCount = g.Count(d => d.Status == DocumentStatus.Approved),
                    RejectedCount = g.Count(d => d.Status == DocumentStatus.Rejected)
                })
                .OrderBy(s => s.Date)
                .ToListAsync();

            return stats;
        }

        // Audit Log - Log Activity
        public async Task LogActivityAsync(string action, string? entityType, int? entityId, string? description, string userId, string? ipAddress = null, string? userAgent = null)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                UserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        // Audit Log - Get Logs
        public async Task<(List<AuditLog> Logs, int TotalCount)> GetAuditLogsAsync(string? userId, string? action, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20)
        {
            try
            {
                // First, check if AuditLogs table exists and has any data (without Include to avoid issues)
                var totalInDb = await _context.AuditLogs.CountAsync();
                System.Diagnostics.Debug.WriteLine($"Total AuditLogs in DB: {totalInDb}");

                // Build base query - start with all logs
                IQueryable<AuditLog> query = _context.AuditLogs;

                // Apply filters only if they are provided
                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(a => a.UserId == userId);
                    System.Diagnostics.Debug.WriteLine($"Applied userId filter: {userId}");
                }

                if (!string.IsNullOrEmpty(action))
                {
                    query = query.Where(a => a.Action == action);
                    System.Diagnostics.Debug.WriteLine($"Applied action filter: {action}");
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp >= fromDate.Value);
                    System.Diagnostics.Debug.WriteLine($"Applied fromDate filter: {fromDate.Value}");
                }

                if (toDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp <= toDate.Value.AddDays(1));
                    System.Diagnostics.Debug.WriteLine($"Applied toDate filter: {toDate.Value}");
                }

                // Count before Include to avoid issues
                var totalCount = await query.CountAsync();
                System.Diagnostics.Debug.WriteLine($"After filters (before Include) - TotalCount: {totalCount}");
                System.Diagnostics.Debug.WriteLine($"Query SQL would be: {query.ToQueryString()}");
                
                // If totalCount is 0 but we know there are records, there might be an issue with the query
                if (totalCount == 0 && totalInDb > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"WARNING: Filters resulted in 0 records but DB has {totalInDb} records!");
                    System.Diagnostics.Debug.WriteLine($"Filters applied - userId: {userId ?? "null"}, action: {action ?? "null"}, fromDate: {fromDate?.ToString() ?? "null"}, toDate: {toDate?.ToString() ?? "null"}");
                    
                    // Try to see what actions exist in the database
                    var allActions = await _context.AuditLogs.Select(a => a.Action).Distinct().ToListAsync();
                    System.Diagnostics.Debug.WriteLine($"Available actions in DB: {string.Join(", ", allActions)}");
                }
                
                // Get logs without Include first to avoid issues
                var logsWithoutInclude = await query
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                System.Diagnostics.Debug.WriteLine($"Logs retrieved (without Include): {logsWithoutInclude.Count}");
                
                // Now manually load Users to avoid Include issues
                if (logsWithoutInclude.Any())
                {
                    var userIds = logsWithoutInclude
                        .Where(l => !string.IsNullOrEmpty(l.UserId))
                        .Select(l => l.UserId)
                        .Distinct()
                        .ToList();
                    
                    if (userIds.Any())
                    {
                        var users = await _userManager.Users
                            .Where(u => userIds.Contains(u.Id))
                            .ToListAsync();
                        
                        var userDict = users.ToDictionary(u => u.Id);
                        
                        // Assign users to logs
                        foreach (var log in logsWithoutInclude)
                        {
                            if (!string.IsNullOrEmpty(log.UserId) && userDict.ContainsKey(log.UserId))
                            {
                                log.User = userDict[log.UserId];
                            }
                        }
                    }
                }
                
                var logs = logsWithoutInclude;

                System.Diagnostics.Debug.WriteLine($"GetAuditLogsAsync: TotalCount={totalCount}, LogsCount={logs?.Count ?? 0}, Page={page}, PageSize={pageSize}");
                
                // If logs is null or empty but totalCount > 0, there might be an issue
                if ((logs == null || logs.Count == 0) && totalCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"WARNING: totalCount={totalCount} but logs is empty! Page={page}, PageSize={pageSize}");
                }

                return (logs ?? new List<AuditLog>(), totalCount);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAuditLogsAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                // Return empty list instead of throwing
                return (new List<AuditLog>(), 0);
            }
        }

        // Document Management - Get All Documents
        public async Task<(List<Document> Documents, int TotalCount)> GetAllDocumentsAsync(string? search, int? courseId, string? status, string? userId, int page = 1, int pageSize = 20)
        {
            var query = _context.Documents
                .Include(d => d.User)
                .Include(d => d.Course)
                .Include(d => d.Folder)
                .Where(d => !d.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.Title.Contains(search) || 
                                        (d.Description != null && d.Description.Contains(search)) ||
                                        (d.FileName != null && d.FileName.Contains(search)));
            }

            if (courseId.HasValue)
            {
                query = query.Where(d => d.CourseId == courseId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<DocumentStatus>(status, out var docStatus))
                {
                    query = query.Where(d => d.Status == docStatus);
                }
            }

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(d => d.UserId == userId);
            }

            var totalCount = await query.CountAsync();
            var documents = await query
                .OrderByDescending(d => d.UploadDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (documents, totalCount);
        }

        // Bulk Operations - Delete
        public async Task<bool> BulkDeleteDocumentsAsync(List<int> documentIds, string deletedBy)
        {
            try
            {
                var documents = await _context.Documents
                    .Where(d => documentIds.Contains(d.Id) && !d.IsDeleted)
                    .ToListAsync();

                foreach (var doc in documents)
                {
                    doc.IsDeleted = true;
                    doc.DeletedDate = DateTime.Now;
                    doc.DeletedBy = deletedBy;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Bulk Operations - Approve
        public async Task<bool> BulkApproveDocumentsAsync(List<int> documentIds, string approvedBy)
        {
            try
            {
                var documents = await _context.Documents
                    .Where(d => documentIds.Contains(d.Id) && !d.IsDeleted)
                    .ToListAsync();

                foreach (var doc in documents)
                {
                    doc.Status = DocumentStatus.Approved;
                    doc.ApprovedBy = approvedBy;
                    doc.ApprovedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Bulk Operations - Reject
        public async Task<bool> BulkRejectDocumentsAsync(List<int> documentIds, string reason, string rejectedBy)
        {
            try
            {
                var documents = await _context.Documents
                    .Where(d => documentIds.Contains(d.Id) && !d.IsDeleted)
                    .ToListAsync();

                foreach (var doc in documents)
                {
                    doc.Status = DocumentStatus.Rejected;
                    doc.RejectionReason = reason;
                    doc.ApprovedBy = rejectedBy;
                    doc.ApprovedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

