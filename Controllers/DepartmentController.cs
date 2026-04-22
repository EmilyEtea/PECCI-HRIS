using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;
using PECCI_HRIS.Models;
using PECCI_HRIS.Services;

namespace PECCI_HRIS.Controllers
{
    [Authorize(Roles = "HR Admin,HR Staff")]
    public class DepartmentController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public DepartmentController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .Include(d => d.Positions)
                .Include(d => d.Employees)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
            return View(departments);
        }

        public IActionResult Create() => View(new Department());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department model)
        {
            if (!ModelState.IsValid) return View(model);

            model.CreatedAt = DateTime.Now;
            _context.Departments.Add(model);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Create", "Department", $"Created department: {model.DepartmentName}", GetClientIP());

            TempData["Success"] = $"Department '{model.DepartmentName}' created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();
            return View(dept);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Department model)
        {
            if (!ModelState.IsValid) return View(model);

            var dept = await _context.Departments.FindAsync(model.DepartmentID);
            if (dept == null) return NotFound();

            dept.DepartmentName = model.DepartmentName;
            dept.DepartmentCode = model.DepartmentCode;
            dept.Description    = model.Description;
            dept.IsActive       = model.IsActive;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Update", "Department", $"Updated department: {dept.DepartmentName}", GetClientIP());

            TempData["Success"] = $"Department '{dept.DepartmentName}' updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();

            dept.IsActive = !dept.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Department '{dept.DepartmentName}' {(dept.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
