using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DMS.Data; // Đảm bảo có namespace này

namespace DMS.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Tiêm DBContext vào để sử dụng
        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh sách môn học từ DB
            var courses = await _context.Courses.ToListAsync();
            return View(courses); // Truyền danh sách sang giao diện
        }
    }
}