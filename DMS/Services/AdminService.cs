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
                    RejectedCount = g.Count(d => d.Status == DocumentStatus.Rejected),
                    ViewCount = g.Sum(d => d.ViewCount),
                    DownloadCount = g.Sum(d => d.DownloadCount)
                })
                .OrderBy(s => s.Date)
                .ToListAsync();

            return stats;
        }

        // Phase 1: Core Reports
        public async Task<DocumentStatusReport> GetDocumentStatusReportAsync()
        {
            var allDocs = await _context.Documents.Where(d => !d.IsDeleted).ToListAsync();
            
            var pending = allDocs.Count(d => d.Status == DocumentStatus.Pending);
            var approved = allDocs.Count(d => d.Status == DocumentStatus.Approved);
            var rejected = allDocs.Count(d => d.Status == DocumentStatus.Rejected);
            var draft = allDocs.Count(d => d.Status == DocumentStatus.Draft);
            
            var totalProcessed = approved + rejected;
            var approvalRate = totalProcessed > 0 ? (double)approved / totalProcessed * 100 : 0;
            var rejectionRate = totalProcessed > 0 ? (double)rejected / totalProcessed * 100 : 0;

            return new DocumentStatusReport
            {
                Pending = pending,
                Approved = approved,
                Rejected = rejected,
                Draft = draft,
                ApprovalRate = approvalRate,
                RejectionRate = rejectionRate
            };
        }

        public async Task<List<DocumentActivityStat>> GetDocumentActivityByWeekAsync(DateTime? fromDate, DateTime? toDate)
        {
            var startDate = fromDate ?? DateTime.Now.AddDays(-84); // 12 weeks default
            var endDate = toDate ?? DateTime.Now;

            var stats = await _context.Documents
                .Where(d => !d.IsDeleted && d.UploadDate >= startDate && d.UploadDate <= endDate)
                .ToListAsync();

            var weeklyStats = stats
                .GroupBy(d => GetWeekStart(d.UploadDate))
                .Select(g => new DocumentActivityStat
                {
                    Date = g.Key,
                    UploadCount = g.Count(),
                    ApprovedCount = g.Count(d => d.Status == DocumentStatus.Approved),
                    RejectedCount = g.Count(d => d.Status == DocumentStatus.Rejected),
                    ViewCount = g.Sum(d => d.ViewCount),
                    DownloadCount = g.Sum(d => d.DownloadCount)
                })
                .OrderBy(s => s.Date)
                .ToList();

            return weeklyStats;
        }

        public async Task<List<DocumentActivityStat>> GetDocumentActivityByMonthAsync(DateTime? fromDate, DateTime? toDate)
        {
            var startDate = fromDate ?? DateTime.Now.AddMonths(-12); // 12 months default
            var endDate = toDate ?? DateTime.Now;

            var stats = await _context.Documents
                .Where(d => !d.IsDeleted && d.UploadDate >= startDate && d.UploadDate <= endDate)
                .ToListAsync();

            var monthlyStats = stats
                .GroupBy(d => new DateTime(d.UploadDate.Year, d.UploadDate.Month, 1))
                .Select(g => new DocumentActivityStat
                {
                    Date = g.Key,
                    UploadCount = g.Count(),
                    ApprovedCount = g.Count(d => d.Status == DocumentStatus.Approved),
                    RejectedCount = g.Count(d => d.Status == DocumentStatus.Rejected),
                    ViewCount = g.Sum(d => d.ViewCount),
                    DownloadCount = g.Sum(d => d.DownloadCount)
                })
                .OrderBy(s => s.Date)
                .ToList();

            return monthlyStats;
        }

        public async Task<PublicShareReport> GetPublicShareReportAsync()
        {
            var allDocs = await _context.Documents.Where(d => !d.IsDeleted).ToListAsync();
            
            var requested = allDocs.Count(d => d.PublicShareRequested);
            var approved = allDocs.Count(d => d.PublicShareApproved);
            var rejected = allDocs.Count(d => d.PublicShareRequested && !d.PublicShareApproved && !string.IsNullOrEmpty(d.RejectionReason));
            
            var approvalRate = requested > 0 ? (double)approved / requested * 100 : 0;

            return new PublicShareReport
            {
                Requested = requested,
                Approved = approved,
                Rejected = rejected,
                ApprovalRate = approvalRate
            };
        }

        // Phase 2: Additional Reports
        public async Task<List<DocumentEngagementStat>> GetDocumentEngagementStatsAsync(int top = 10)
        {
            var stats = await _context.Documents
                .Include(d => d.Course)
                .Where(d => !d.IsDeleted)
                .Select(d => new DocumentEngagementStat
                {
                    DocumentId = d.Id,
                    DocumentTitle = d.Title,
                    CourseName = d.Course != null ? d.Course.CourseName : "Không có",
                    ViewCount = d.ViewCount,
                    DownloadCount = d.DownloadCount,
                    ConversionRate = d.ViewCount > 0 ? (double)d.DownloadCount / d.ViewCount * 100 : 0
                })
                .OrderByDescending(s => s.ViewCount)
                .ThenByDescending(s => s.DownloadCount)
                .Take(top)
                .ToListAsync();

            return stats;
        }

        public async Task<List<UserByFacultyStat>> GetUsersByFacultyStatsAsync()
        {
            var users = await _userManager.Users
                .Include(u => u.Documents)
                .ToListAsync();

            var stats = users
                .GroupBy(u => u.Faculty ?? "Không xác định")
                .Select(g => new UserByFacultyStat
                {
                    Faculty = g.Key,
                    UserCount = g.Count(),
                    DocumentCount = g.Sum(u => u.Documents.Count(d => !d.IsDeleted)),
                    TotalStorage = g.Sum(u => u.Documents.Where(d => !d.IsDeleted).Sum(d => d.FileSize))
                })
                .OrderByDescending(s => s.UserCount)
                .ToList();

            return stats;
        }

        public async Task<StorageDetailReport> GetStorageDetailReportAsync()
        {
            var allDocs = await _context.Documents
                .Include(d => d.Course)
                .Include(d => d.User)
                .Where(d => !d.IsDeleted)
                .ToListAsync();

            var totalStorage = allDocs.Sum(d => d.FileSize);

            // By Course
            var byCourse = allDocs
                .Where(d => d.Course != null)
                .GroupBy(d => d.Course!.CourseName)
                .Select(g => new StorageByCourse
                {
                    CourseName = g.Key,
                    Storage = g.Sum(d => d.FileSize),
                    Percentage = totalStorage > 0 ? (double)g.Sum(d => d.FileSize) / totalStorage * 100 : 0
                })
                .OrderByDescending(s => s.Storage)
                .Take(10)
                .ToList();

            // By Faculty
            var byFaculty = allDocs
                .Where(d => d.User != null && !string.IsNullOrEmpty(d.User.Faculty))
                .GroupBy(d => d.User!.Faculty!)
                .Select(g => new StorageByFaculty
                {
                    Faculty = g.Key,
                    Storage = g.Sum(d => d.FileSize),
                    Percentage = totalStorage > 0 ? (double)g.Sum(d => d.FileSize) / totalStorage * 100 : 0
                })
                .OrderByDescending(s => s.Storage)
                .ToList();

            // By File Type
            var byFileType = allDocs
                .GroupBy(d => d.ContentType)
                .Select(g => new StorageByFileType
                {
                    FileType = g.Key,
                    Storage = g.Sum(d => d.FileSize),
                    Percentage = totalStorage > 0 ? (double)g.Sum(d => d.FileSize) / totalStorage * 100 : 0
                })
                .OrderByDescending(s => s.Storage)
                .Take(10)
                .ToList();

            return new StorageDetailReport
            {
                TotalStorage = totalStorage,
                ByCourse = byCourse,
                ByFaculty = byFaculty,
                ByFileType = byFileType
            };
        }

        // Phase 3: Advanced Reports
        public async Task<QuizReport> GetQuizReportAsync()
        {
            var allQuizzes = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Attempts)
                .Where(q => !q.IsDeleted)
                .ToListAsync();

            var totalQuizzes = allQuizzes.Count;
            var publishedQuizzes = allQuizzes.Count(q => q.IsPublished);
            var unpublishedQuizzes = totalQuizzes - publishedQuizzes;
            var totalAttempts = allQuizzes.Sum(q => q.Attempts.Count);
            
            var completedAttempts = allQuizzes
                .SelectMany(q => q.Attempts)
                .Where(a => a.SubmittedAt.HasValue)
                .ToList();
            
            var averageScore = completedAttempts.Any() 
                ? completedAttempts.Where(a => a.ScorePercentage.HasValue).Average(a => a.ScorePercentage!.Value) 
                : 0;
            
            var completionRate = totalAttempts > 0 
                ? (double)completedAttempts.Count / totalAttempts * 100 
                : 0;

            // By Course
            var byCourse = allQuizzes
                .Where(q => q.Course != null)
                .GroupBy(q => q.Course!.CourseName)
                .Select(g => new QuizByCourse
                {
                    CourseName = g.Key,
                    QuizCount = g.Count(),
                    AttemptCount = g.Sum(q => q.Attempts.Count),
                    AverageScore = g.SelectMany(q => q.Attempts)
                        .Where(a => a.ScorePercentage.HasValue)
                        .Any() 
                        ? g.SelectMany(q => q.Attempts)
                            .Where(a => a.ScorePercentage.HasValue)
                            .Average(a => a.ScorePercentage!.Value)
                        : 0
                })
                .OrderByDescending(s => s.QuizCount)
                .ToList();

            return new QuizReport
            {
                TotalQuizzes = totalQuizzes,
                PublishedQuizzes = publishedQuizzes,
                UnpublishedQuizzes = unpublishedQuizzes,
                TotalAttempts = totalAttempts,
                AverageScore = averageScore,
                CompletionRate = completionRate,
                ByCourse = byCourse
            };
        }

        public async Task<AuditLogReport> GetAuditLogReportAsync(DateTime? fromDate, DateTime? toDate)
        {
            var startDate = fromDate ?? DateTime.Now.AddDays(-30);
            var endDate = toDate ?? DateTime.Now;

            var logs = await _context.AuditLogs
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .ToListAsync();

            var totalLogs = logs.Count;

            // By Action
            var byAction = logs
                .GroupBy(l => l.Action)
                .ToDictionary(g => g.Key, g => g.Count());

            // By Entity Type
            var byEntityType = logs
                .Where(l => !string.IsNullOrEmpty(l.EntityType))
                .GroupBy(l => l.EntityType!)
                .ToDictionary(g => g.Key, g => g.Count());

            // Top Users
            var topUsers = logs
                .GroupBy(l => l.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var userIds = topUsers.Select(u => u.UserId).ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            var userDict = users.ToDictionary(u => u.Id);

            var topUserActivities = topUsers
                .Select(u => new TopUserActivity
                {
                    UserName = userDict.ContainsKey(u.UserId) ? userDict[u.UserId].FullName : "Unknown",
                    UserEmail = userDict.ContainsKey(u.UserId) ? userDict[u.UserId].Email ?? "" : "",
                    ActivityCount = u.Count
                })
                .ToList();

            return new AuditLogReport
            {
                TotalLogs = totalLogs,
                ByAction = byAction,
                ByEntityType = byEntityType,
                TopUsers = topUserActivities
            };
        }

        // Helper method
        private DateTime GetWeekStart(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
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

