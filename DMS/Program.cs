using DMS.Data;
using DMS.Models; // Đảm bảo có namespace này để dùng ApplicationUser
using DMS.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DMS.Services.Interfaces;
using Microsoft.AspNetCore.Http;

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
    
    // Cấu hình SignIn options
    options.SignIn.RequireConfirmedEmail = false; // Không yêu cầu xác nhận email
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 4. Cấu hình Cookie Authentication cho Remember Me
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie sẽ tồn tại 30 ngày nếu user chọn "Remember Me"
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    // Tự động gia hạn cookie khi user còn hoạt động
    options.SlidingExpiration = true;
    // Đường dẫn login và logout
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    // Cookie chỉ được gửi qua HTTPS trong production
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    // Cookie chỉ có thể truy cập qua HTTP (không qua JavaScript)
    options.Cookie.HttpOnly = true;
    // Tên cookie
    options.Cookie.Name = "DMS.Auth";
    // Cookie sẽ tồn tại ngay cả khi browser đóng (nếu Remember Me)
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 4. Đăng ký các Service nội bộ (Dependency Injection)
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Cấu hình để tăng giới hạn kích thước request cho upload nhiều file
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524288000; // 500MB
    options.ValueLengthLimit = int.MaxValue;
    options.ValueCountLimit = int.MaxValue;
    options.KeyLengthLimit = int.MaxValue;
});

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

// Default route: vào trang login trước
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// 6. Tự động apply migrations và seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Tự động apply migrations nếu chưa được apply
        logger.LogInformation("Đang kiểm tra và áp dụng migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Migrations đã được áp dụng thành công.");
        
        // Gọi hàm Seed Data
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Một lỗi đã xảy ra khi khởi tạo database hoặc nạp dữ liệu mẫu.");
    }
}

app.Run();