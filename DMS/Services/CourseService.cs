using DMS.Data;
using DMS.Models;
using DMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get All Courses
        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .ToListAsync();
        }

        // Get Courses by Instructor
        public async Task<List<Course>> GetCoursesByInstructorAsync(string instructorId)
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();
        }

        // Get Course by ID
        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Get Course with Documents
        public async Task<Course?> GetCourseWithDocumentsAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Documents)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Create Course
        public async Task<Course> CreateCourseAsync(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        // Update Course
        public async Task<bool> UpdateCourseAsync(Course course)
        {
            try
            {
                _context.Courses.Update(course);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Delete Course
        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return false;
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

