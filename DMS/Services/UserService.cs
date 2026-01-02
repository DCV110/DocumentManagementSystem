using DMS.Data;
using DMS.Models;
using DMS.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DMS.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Get Users with pagination
        public async Task<(List<ApplicationUser> Users, int TotalCount)> GetUsersAsync(
            string? search,
            string? role,
            int page = 1,
            int pageSize = 20)
        {
            var query = _userManager.Users.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) ||
                                        (u.Email != null && u.Email.Contains(search)) ||
                                        (u.StudentCode != null && u.StudentCode.Contains(search)));
            }

            // Role filter (TODO: Implement when needed)
            // if (!string.IsNullOrEmpty(role))
            // {
            //     query = query.Where(...);
            // }

            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        // Get Users with Roles
        public async Task<List<object>> GetUsersWithRolesAsync(
            string? search,
            string? role,
            int page = 1,
            int pageSize = 20)
        {
            var (users, _) = await GetUsersAsync(search, role, page, pageSize);

            var usersWithRoles = new List<object>();
            foreach (var user in users)
            {
                if (user != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    usersWithRoles.Add(new
                    {
                        User = user,
                        Roles = userRoles
                    });
                }
            }

            return usersWithRoles;
        }

        // Create User
        public async Task<IdentityResult> CreateUserAsync(
            string email,
            string password,
            string fullName,
            string? role,
            string? studentCode,
            string? faculty,
            string? classCode = null)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                StudentCode = studentCode,
                Faculty = faculty,
                EmailConfirmed = true,
                CreatedDate = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded && !string.IsNullOrEmpty(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return result;
        }

        // Update User
        public async Task<IdentityResult> UpdateUserAsync(
            string userId,
            string email,
            string fullName,
            string? role,
            string? studentCode,
            string? faculty,
            string? classCode = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            user.Email = email;
            user.UserName = email;
            user.FullName = fullName;
            user.StudentCode = studentCode;
            user.Faculty = faculty;

            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded && !string.IsNullOrEmpty(role))
            {
                // Remove all existing roles
                var existingRoles = await _userManager.GetRolesAsync(user);
                if (existingRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, existingRoles);
                }
                // Add new role
                await _userManager.AddToRoleAsync(user, role);
            }

            return result;
        }

        // Toggle User Lock
        public async Task<bool> ToggleUserLockAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.LockoutEnd = user.LockoutEnd.HasValue 
                ? null 
                : DateTimeOffset.UtcNow.AddYears(100);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        // Assign Role
        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            
            if (!string.IsNullOrEmpty(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return true;
        }

        // Get All Roles
        public async Task<List<IdentityRole>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.ToListAsync();
        }

        // Get Total Users Count
        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _userManager.Users.CountAsync();
        }
    }
}

