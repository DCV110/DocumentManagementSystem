using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DMS.Data;
using DMS.Models;

namespace DMS.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class FolderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FolderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Folder
        public async Task<IActionResult> Index()
        {
            var folders = await _context.Folders
                .Include(f => f.Course)
                .Include(f => f.Documents)
                .ToListAsync();

            var courses = await _context.Courses.ToListAsync();

            ViewBag.Folders = folders;
            ViewBag.Courses = courses;

            return View();
        }

        // GET: Folder/Create
        public async Task<IActionResult> Create()
        {
            var courses = await _context.Courses.ToListAsync();
            ViewBag.Courses = courses;
            return View();
        }

        // POST: Folder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FolderName,CourseId")] Folder folder)
        {
            if (ModelState.IsValid)
            {
                folder.CreatedDate = DateTime.Now;
                _context.Add(folder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var courses = await _context.Courses.ToListAsync();
            ViewBag.Courses = courses;
            return View(folder);
        }

        // GET: Folder/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var folder = await _context.Folders.FindAsync(id);
            if (folder == null)
            {
                return NotFound();
            }

            var courses = await _context.Courses.ToListAsync();
            ViewBag.Courses = courses;
            return View(folder);
        }

        // POST: Folder/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FolderName,CourseId,CreatedDate")] Folder folder)
        {
            if (id != folder.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(folder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FolderExists(folder.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var courses = await _context.Courses.ToListAsync();
            ViewBag.Courses = courses;
            return View(folder);
        }

        // POST: Folder/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var folder = await _context.Folders.FindAsync(id);
            if (folder != null)
            {
                _context.Folders.Remove(folder);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Folder/Permissions/5
        public async Task<IActionResult> Permissions(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var folder = await _context.Folders.FindAsync(id);
            if (folder == null)
            {
                return NotFound();
            }

            // Lấy danh sách users để phân quyền
            var users = await _userManager.Users.ToListAsync();
            ViewBag.Folder = folder;
            ViewBag.Users = users;

            return View();
        }

        private bool FolderExists(int id)
        {
            return _context.Folders.Any(e => e.Id == id);
        }
    }
}

