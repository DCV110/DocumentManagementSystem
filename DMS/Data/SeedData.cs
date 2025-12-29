using DMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DMS.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // 1. Kiểm tra và tạo Môn học mẫu
                if (!context.Courses.Any())
                {
                    context.Courses.AddRange(
                        new Course { CourseName = "Lập trình C# cơ bản", CourseCode = "IT001", Description = "Nhập môn lập trình .NET" },
                        new Course { CourseName = "Cấu trúc dữ liệu và Giải thuật", CourseCode = "IT002", Description = "Các thuật toán cơ bản" },
                        new Course { CourseName = "Cơ sở dữ liệu SQL Server", CourseCode = "IT003", Description = "Quản lý dữ liệu hệ thống" }
                    );
                    await context.SaveChangesAsync();
                }

                // 2. Tạo Tài khoản Admin mẫu (Sử dụng Identity)
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Tạo Role Admin nếu chưa có
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Tạo User Admin
                string adminEmail = "admin@dms.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "Hệ Thống Admin",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Mật khẩu mẫu
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
            }
        }
    }
}