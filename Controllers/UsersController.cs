using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;
using PECCI_HRIS.Models;
using PECCI_HRIS.Services;
using PECCI_HRIS.ViewModels;

namespace PECCI_HRIS.Controllers
{
    // HR Staff can view users but not create/edit/delete — write actions stay HR Admin only
    [Authorize(Roles = "HR Admin,HR Staff")]
    public class UsersController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public UsersController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Employee)
                .OrderBy(u => u.Username)
                .ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new CreateUserViewModel
            {
                Roles     = await GetRoleList(),
                Employees = await GetEmployeeList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> Create(CreateUserViewModel vm)
        {
            if (await _context.Users.AnyAsync(u => u.Username == vm.Username))
                ModelState.AddModelError("Username", "Username already exists.");

            if (await _context.Users.AnyAsync(u => u.Email == vm.Email))
                ModelState.AddModelError("Email", "Email already in use.");

            if (!ModelState.IsValid)
            {
                vm.Roles     = await GetRoleList();
                vm.Employees = await GetEmployeeList();
                return View(vm);
            }

            var user = new User
            {
                Username     = vm.Username,
                Email        = vm.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password),
                RoleID       = vm.RoleID,
                EmployeeID   = vm.EmployeeID,
                IsActive     = vm.IsActive,
                CreatedAt    = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Create", "Users", $"Created user: {user.Username}", GetClientIP());

            TempData["Success"] = $"User '{user.Username}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var vm = new UserViewModel
            {
                UserID     = user.UserID,
                Username   = user.Username,
                Email      = user.Email,
                RoleID     = user.RoleID,
                EmployeeID = user.EmployeeID,
                IsActive   = user.IsActive,
                Roles      = await GetRoleList(),
                Employees  = await GetEmployeeList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> Edit(UserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Roles     = await GetRoleList();
                vm.Employees = await GetEmployeeList();
                return View(vm);
            }

            var user = await _context.Users.FindAsync(vm.UserID);
            if (user == null) return NotFound();

            user.Email      = vm.Email;
            user.RoleID     = vm.RoleID;
            user.EmployeeID = vm.EmployeeID;
            user.IsActive   = vm.IsActive;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Update", "Users", $"Updated user: {user.Username}", GetClientIP());

            TempData["Success"] = $"User '{user.Username}' updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(new ChangePasswordViewModel { UserID = id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> ResetPassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _context.Users.FindAsync(vm.UserID);
            if (user == null) return NotFound();

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "ResetPassword", "Users", $"Reset password for user: {user.Username}", GetClientIP());

            TempData["Success"] = $"Password reset for '{user.Username}'.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.UserID == GetCurrentUserID())
            {
                TempData["Error"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User '{user.Username}' {(user.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetRoleList() =>
            await _context.Roles
                .Where(r => r.IsActive)
                .Select(r => new SelectListItem { Value = r.RoleID.ToString(), Text = r.RoleName })
                .ToListAsync();

        private async Task<IEnumerable<SelectListItem>> GetEmployeeList() =>
            await _context.Employees
                .Where(e => e.Status == "Active")
                .OrderBy(e => e.LastName)
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeID.ToString(),
                    Text  = $"{e.FullName} ({e.EmployeeNo})"
                })
                .ToListAsync();
    }
}
