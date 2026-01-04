using DMS.Models;

namespace DMS.Services.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(string userId, string title, string? message = null, string? type = null, string? actionUrl = null, string? entityType = null, int? entityId = null);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId, string userId);
    }
}

