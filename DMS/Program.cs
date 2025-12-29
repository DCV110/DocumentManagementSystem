using DMS.Data;
using DMS.Models; // Đảm bảo có namespace này để dùng ApplicationUser
using DMS.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DMS.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. ĐĂNG KÝ IDENTITY (Bổ sung phần này)
// Việc đăng ký này giúp hệ thống quản lý ApplicationUser và Role thông qua DB
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = false; // Tùy chỉnh độ khó mật khẩu nếu cần
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 4. Đăng ký các Service nội bộ (Dependency Injection)
// Bạn nên đăng ký ở đây để các Controller có thể sử dụng DocumentService
builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5. BẬT XÁC THỰC (Bổ sung UseAuthentication)
app.UseAuthentication(); // Phải nằm TRƯỚC UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Course}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Gọi hàm Seed Data
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Một lỗi đã xảy ra khi nạp dữ liệu mẫu.");
    }
}

app.Run();