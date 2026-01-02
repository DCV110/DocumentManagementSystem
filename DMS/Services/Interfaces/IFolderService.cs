using DMS.Models;

namespace DMS.Services.Interfaces
{
    public interface IFolderService
    {
        // Get Operations
        Task<List<Folder>> GetAllFoldersAsync();
        Task<List<Folder>> GetFoldersByUserAsync(string userId);
        Task<Folder?> GetFolderByIdAsync(int id);
        Task<Folder?> GetFolderWithDetailsAsync(int id);
        Task<List<Folder>> GetFoldersByCourseAsync(int courseId);
        
        // CRUD Operations
        Task<Folder> CreateFolderAsync(Folder folder);
        Task<bool> UpdateFolderAsync(Folder folder);
        Task<bool> DeleteFolderAsync(int id, string deletedBy);
        Task<bool> FolderExistsAsync(int id);
        
        // Soft Delete Operations
        Task<List<Folder>> GetDeletedFoldersAsync(string userId);
        Task<bool> RestoreFolderAsync(int id);
        Task<bool> DeletePermanentlyAsync(int id);
        
        // Helper Methods
        Task<List<Course>> GetAllCoursesAsync();
    }
}

