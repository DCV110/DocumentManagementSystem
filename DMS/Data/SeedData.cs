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

                // 2. Tạo Roles và Users mẫu
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Tạo các Roles
                string[] roles = { "Admin", "Instructor", "Student" };
                foreach (var roleName in roles)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
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

                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }

                // Tạo User Giảng viên
                string instructorEmail = "instructor@dms.com";
                if (await userManager.FindByEmailAsync(instructorEmail) == null)
                {
                    var instructorUser = new ApplicationUser
                    {
                        UserName = instructorEmail,
                        Email = instructorEmail,
                        FullName = "TS. Nguyễn Văn A",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(instructorUser, "Instructor@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(instructorUser, "Instructor");
                    }
                }

                // Tạo User Sinh viên
                string studentEmail = "student@dms.com";
                ApplicationUser? studentUser = null;
                if (await userManager.FindByEmailAsync(studentEmail) == null)
                {
                    studentUser = new ApplicationUser
                    {
                        UserName = studentEmail,
                        Email = studentEmail,
                        FullName = "Nguyễn Văn B",
                        StudentCode = "SV001",
                        Faculty = "Khoa CNTT",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(studentUser, "Student@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(studentUser, "Student");
                    }
                }
                else
                {
                    studentUser = await userManager.FindByEmailAsync(studentEmail);
                }

                // Tạo thêm Giảng viên và Sinh viên mẫu
                var instructor2Email = "instructor2@dms.com";
                ApplicationUser? instructor2 = null;
                if (await userManager.FindByEmailAsync(instructor2Email) == null)
                {
                    instructor2 = new ApplicationUser
                    {
                        UserName = instructor2Email,
                        Email = instructor2Email,
                        FullName = "ThS. Lê Thị Lan",
                        Faculty = "Khoa Hóa",
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(instructor2, "Instructor@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(instructor2, "Instructor");
                    }
                }
                else
                {
                    instructor2 = await userManager.FindByEmailAsync(instructor2Email);
                }

                var instructor3Email = "instructor3@dms.com";
                ApplicationUser? instructor3 = null;
                if (await userManager.FindByEmailAsync(instructor3Email) == null)
                {
                    instructor3 = new ApplicationUser
                    {
                        UserName = instructor3Email,
                        Email = instructor3Email,
                        FullName = "TS. Trần Hùng",
                        Faculty = "Khoa CNTT",
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(instructor3, "Instructor@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(instructor3, "Instructor");
                    }
                }
                else
                {
                    instructor3 = await userManager.FindByEmailAsync(instructor3Email);
                }

                await context.SaveChangesAsync();

                // 3. Tạo Folders mẫu - Đã xóa, không tạo folders mẫu
                // Người dùng sẽ tự tạo folders khi cần

                // 4. Tạo Documents mẫu - Đã xóa, không tạo documents mẫu
                // Người dùng sẽ tự tải documents khi cần
            }
        }
    }
}