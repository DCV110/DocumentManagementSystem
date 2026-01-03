using DMS.Models;

namespace DMS.Services.Interfaces
{
    public interface IBackupService
    {
        Task<(bool Success, string? ErrorMessage, BackupRecord? BackupRecord)> CreateBackupAsync(string userId, string? description = null);
        Task<List<BackupRecord>> GetAllBackupsAsync();
        Task<BackupRecord?> GetBackupByIdAsync(int id);
        Task<bool> DeleteBackupAsync(int id);
        Task<(bool Success, string? ErrorMessage)> RestoreBackupAsync(int backupId, string userId, string? notes = null);
        Task<bool> BackupExistsAsync(int id);
    }
}

