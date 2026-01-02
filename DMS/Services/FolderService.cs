using DMS.Data;
using DMS.Models;
using DMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS.Services
{
    public class FolderService : IFolderService
    {
        private readonly ApplicationDbContext _context;

        public FolderService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get All Folders
        public async Task<List<Folder>> GetAllFoldersAsync()
        {
            return await _context.Folders
                .Include(f => f.Course)
                .Include(f => f.Documents)
                .Where(f => !f.IsDeleted)
                .ToListAsync();
        }

        // Get Folders by User (created by user)
        public async Task<List<Folder>> GetFoldersByUserAsync(string userId)
        {
            return await _context.Folders
                .Include(f => f.Course)
                .Include(f => f.Documents)
                .Where(f => f.CreatedBy == userId && !f.IsDeleted)
                .ToListAsync();
        }

        // Get Folder by ID
        public async Task<Folder?> GetFolderByIdAsync(int id)
        {
            return await _context.Folders
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        // Get Folder with Details
        public async Task<Folder?> GetFolderWithDetailsAsync(int id)
        {
            return await _context.Folders
                .IgnoreQueryFilters()
                .Include(f => f.Course)
                .Include(f => f.Documents)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        // Get Folders by Course
        public async Task<List<Folder>> GetFoldersByCourseAsync(int courseId)
        {
            return await _context.Folders
                .Where(f => f.CourseId == courseId && !f.IsDeleted)
                .ToListAsync();
        }

        // Create Folder
        public async Task<Folder> CreateFolderAsync(Folder folder)
        {
            folder.CreatedDate = DateTime.Now;
            _context.Add(folder);
            await _context.SaveChangesAsync();
            return folder;
        }

        // Update Folder
        public async Task<bool> UpdateFolderAsync(Folder folder)
        {
            try
            {
                _context.Update(folder);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await FolderExistsAsync(folder.Id))
                {
                    return false;
                }
                throw;
            }
        }

        // Delete Folder (Soft Delete)
        public async Task<bool> DeleteFolderAsync(int id, string deletedBy)
        {
            var folder = await GetFolderByIdAsync(id);
            if (folder == null || folder.IsDeleted)
            {
                return false;
            }

            folder.IsDeleted = true;
            folder.DeletedDate = DateTime.Now;
            folder.DeletedBy = deletedBy;
            
            await _context.SaveChangesAsync();
            return true;
        }

        // Get Deleted Folders by User
        public async Task<List<Folder>> GetDeletedFoldersAsync(string userId)
        {
            return await _context.Folders
                .IgnoreQueryFilters()
                .Include(f => f.Course)
                .Include(f => f.Documents)
                .Where(f => f.CreatedBy == userId && f.IsDeleted)
                .OrderByDescending(f => f.DeletedDate)
                .ToListAsync();
        }

        // Restore Folder
        public async Task<bool> RestoreFolderAsync(int id)
        {
            var folder = await GetFolderByIdAsync(id);
            if (folder == null || !folder.IsDeleted)
            {
                return false;
            }

            folder.IsDeleted = false;
            folder.DeletedDate = null;
            folder.DeletedBy = null;
            
            await _context.SaveChangesAsync();
            return true;
        }

        // Delete Folder Permanently (Hard Delete)
        public async Task<bool> DeletePermanentlyAsync(int id)
        {
            var folder = await _context.Folders
                .IgnoreQueryFilters()
                .Include(f => f.Documents)
                .FirstOrDefaultAsync(f => f.Id == id);
            
            if (folder == null)
            {
                return false;
            }

            // Note: Documents in the folder will have their FolderId set to null
            // If you want to delete documents too, uncomment the following:
            // _context.Documents.RemoveRange(folder.Documents);

            // Delete folder from database
            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();
            return true;
        }

        // Check if Folder Exists
        public async Task<bool> FolderExistsAsync(int id)
        {
            return await _context.Folders.AnyAsync(e => e.Id == id);
        }

        // Helper: Get All Courses
        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.ToListAsync();
        }
    }
}

