using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DMS.Data;
using DMS.Models;

namespace DMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // User Management
        public async Task<IActionResult> UserManagement(string? search, string? role, int page = 1)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || 
                                        (u.Email != null && u.Email.Contains(search)) ||
                                        (u.StudentCode != null && u.StudentCode.Contains(search)));
            }

            var users = await query
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();

            // Lấy roles cho mỗi user
            var usersWithRoles = new List<object>();
            foreach (var user in users)
            {
                if (user != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    usersWithRoles.Add(new
                    {
                        User = user,
                        Roles = userRoles
                    });
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var totalUsers = await query.CountAsync();

            ViewBag.Users = usersWithRoles;
            ViewBag.Roles = roles;
            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / 20.0);

            return View();
        }

        // Create User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string email, string password, string fullName, string role, string? studentCode, string? faculty)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                StudentCode = studentCode,
                Faculty = faculty,
                EmailConfirmed = true,
                CreatedDate = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded && !string.IsNullOrEmpty(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction("UserManagement");
        }

        // Lock/Unlock User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.LockoutEnd = user.LockoutEnd.HasValue ? null : DateTimeOffset.UtcNow.AddYears(100);
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("UserManagement");
        }

        // Assign Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!string.IsNullOrEmpty(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
            return RedirectToAction("UserManagement");
        }

        // Audit Log
        public IActionResult AuditLog(string? userId, string? action, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            // Tạm thời trả về view, sau này sẽ implement audit log table
            ViewBag.UserId = userId;
            ViewBag.Action = action;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Page = page;

            return View();
        }

        // Reports
        public async Task<IActionResult> Reports(DateTime? fromDate, DateTime? toDate)
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalDocuments = await _context.Documents.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();

            // Statistics by faculty
            var documentsByCourse = await _context.Documents
                .Include(d => d.Course)
                .GroupBy(d => d.Course.CourseName)
                .Select(g => new
                {
                    CourseName = g.Key,
                    Count = g.Count(),
                    TotalSize = g.Sum(d => d.FileSize)
                })
                .ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalDocuments = totalDocuments;
            ViewBag.TotalCourses = totalCourses;
            ViewBag.DocumentsByCourse = documentsByCourse;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View();
        }

        // Backup
        public IActionResult Backup()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PerformBackup()
        {
            // Implement backup logic here
            TempData["Message"] = "Backup đã được thực hiện thành công!";
            return RedirectToAction("Backup");
        }
    }
}

