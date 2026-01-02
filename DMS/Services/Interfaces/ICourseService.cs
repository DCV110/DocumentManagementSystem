using DMS.Models;

namespace DMS.Services.Interfaces
{
    public interface ICourseService
    {
        // Get Operations
        Task<List<Course>> GetAllCoursesAsync();
        Task<List<Course>> GetCoursesByInstructorAsync(string instructorId);
        Task<Course?> GetCourseByIdAsync(int id);
        Task<Course?> GetCourseWithDocumentsAsync(int id);
        
        // CRUD Operations
        Task<Course> CreateCourseAsync(Course course);
        Task<bool> UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(int id);
    }
}

