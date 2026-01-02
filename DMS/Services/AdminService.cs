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
    }
}

