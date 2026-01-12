using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DMS.Models;
using DMS.Services.Interfaces;

namespace DMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAdminService _adminService;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IAdminService adminService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _adminService = adminService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập email và mật khẩu.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View();
            }

            // Sử dụng PasswordSignInAsync với isPersistent = rememberMe
            // isPersistent = true: Cookie sẽ tồn tại lâu (theo ExpireTimeSpan trong cookie config)
            // isPersistent = false: Cookie sẽ là session cookie (hết khi đóng browser)
            var result = await _signInManager.PasswordSignInAsync(
                user, 
                password, 
                isPersistent: rememberMe, // Remember Me checkbox
                lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                // Log login activity
                await _adminService.LogActivityAsync(
                    "Login",
                    "User",
                    null,
                    $"Đã đăng nhập vào hệ thống",
                    user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );
                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa. Vui lòng thử lại sau.");
                return View();
            }

            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Log logout activity before signing out
            if (user != null)
            {
                await _adminService.LogActivityAsync(
                    "Logout",
                    "User",
                    null,
                    $"Đã đăng xuất khỏi hệ thống",
                    user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );
            }
            
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}

