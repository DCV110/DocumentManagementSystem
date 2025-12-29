using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DMS.Models;
using DMS.Data;

namespace DMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
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

            // Lấy thông tin cho dashboard sinh viên
            var courses = _context.Courses.Take(3).ToList();
            var documents = _context.Documents
                .Where(d => d.UserId == user.Id)
                .OrderByDescending(d => d.UploadDate)
                .Take(4)
                .ToList();

            ViewBag.User = user;
            ViewBag.Courses = courses;
            ViewBag.Documents = documents;

            return View();
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> InstructorDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy thông tin cho dashboard giảng viên
            var courses = _context.Courses.Take(3).ToList();
            var documents = _context.Documents
                .OrderByDescending(d => d.UploadDate)
                .Take(4)
                .ToList();

            ViewBag.User = user;
            ViewBag.Courses = courses;
            ViewBag.Documents = documents;

            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy thông tin cho dashboard admin
            var totalUsers = _userManager.Users.Count();
            var totalDocuments = _context.Documents.Count();
            var totalCourses = _context.Courses.Count();

            ViewBag.User = user;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalDocuments = totalDocuments;
            ViewBag.TotalCourses = totalCourses;

            return View();
        }

        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> MyDocuments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy tài liệu của user hiện tại
            var documents = _context.Documents
                .Where(d => d.UserId == user.Id)
                .OrderByDescending(d => d.UploadDate)
                .ToList();

            ViewBag.User = user;
            ViewBag.Documents = documents;

            return View();
        }

        // My Courses - Student
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy danh sách khóa học (tạm thời lấy tất cả, sau này sẽ filter theo user)
            var courses = await _context.Courses
                .Include(c => c.Documents)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.Courses = courses;

            return View();
        }

        // Schedule - Student
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Schedule()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy danh sách khóa học để hiển thị lịch
            var courses = await _context.Courses.ToListAsync();

            ViewBag.User = user;
            ViewBag.Courses = courses;

            return View();
        }
    }
}

