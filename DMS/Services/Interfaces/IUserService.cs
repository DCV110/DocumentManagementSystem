using DMS.Models;
using Microsoft.AspNetCore.Identity;

namespace DMS.Services.Interfaces
{
    public interface IUserService
    {
        // Get Users
        Task<(List<ApplicationUser> Users, int TotalCount)> GetUsersAsync(
            string? search, 
            string? role, 
            int page = 1, 
            int pageSize = 20);
        
        Task<List<object>> GetUsersWithRolesAsync(
            string? search, 
            string? role, 
            int page = 1, 
            int pageSize = 20);
        
        // User Operations
        Task<IdentityResult> CreateUserAsync(
            string email, 
            string password, 
            string fullName, 
            string? role, 
            string? studentCode, 
            string? faculty,
            string? classCode = null);
        
        Task<IdentityResult> UpdateUserAsync(
            string userId,
            string email,
            string fullName,
            string? role,
            string? studentCode,
            string? faculty,
            string? classCode = null);
        
        Task<bool> ToggleUserLockAsync(string userId);
        Task<bool> AssignRoleAsync(string userId, string role);
        
        // Helper Methods
        Task<List<IdentityRole>> GetAllRolesAsync();
        Task<int> GetTotalUsersCountAsync();
    }
}

