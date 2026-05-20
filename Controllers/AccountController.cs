using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;
using PECCI_HRIS.Services;
using PECCI_HRIS.ViewModels;
using System.Security.Claims;

namespace PECCI_HRIS.Controllers
{
    public class AccountController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public AccountController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                await _auditService.LogAsync(null, model.Username, "FailedLogin", "Account",
                    $"Failed login attempt for username: {model.Username}", GetClientIP());
                return View(model);
            }

            // Build claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role?.RoleName ?? "Employee"),
                new("FullName", user.Employee?.DisplayName ?? user.Username),
                new("EmployeeID", user.EmployeeID?.ToString() ?? "0")
            };

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc   = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            // Update last login
            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(user.UserID, user.Username, "Login", "Account",
                "User logged in successfully", GetClientIP());

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Logout", "Account", "User logged out", GetClientIP());

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied() => View();

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            int userId = GetCurrentUserID();
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Employee).ThenInclude(e => e!.Department)
                .Include(u => u.Employee).ThenInclude(e => e!.Position)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null) return NotFound();
            return View(user);
        }

        // ── Change Password (self-service) ────────────────────────────────────────

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel { UserID = GetCurrentUserID() });
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _context.Users.FindAsync(GetCurrentUserID());
            if (user == null) return NotFound();

            // Verify current password before allowing change
            if (!BCrypt.Net.BCrypt.Verify(vm.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(vm);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "ChangePassword", "Account", "User changed their own password", GetClientIP());

            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction(nameof(Profile));
        }
    }
}
