using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;
using PECCI_HRIS.Models;
using PECCI_HRIS.Services;

namespace PECCI_HRIS.Controllers
{
    [Authorize(Roles = "HR Admin,HR Staff")]
    public class PositionController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public PositionController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var positions = await _context.Positions
                .Include(p => p.Department)
                .Include(p => p.Employees)
                .OrderBy(p => p.Department!.DepartmentName)
                .ThenBy(p => p.PositionTitle)
                .ToListAsync();
            return View(positions);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await GetDepartmentList();
            return View(new Position());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Position model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await GetDepartmentList();
                return View(model);
            }

            model.CreatedAt = DateTime.Now;
            _context.Positions.Add(model);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Create", "Position", $"Created position: {model.PositionTitle}", GetClientIP());

            TempData["Success"] = $"Position '{model.PositionTitle}' created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            if (position == null) return NotFound();
            ViewBag.Departments = await GetDepartmentList();
            return View(position);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Position model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await GetDepartmentList();
                return View(model);
            }

            var position = await _context.Positions.FindAsync(model.PositionID);
            if (position == null) return NotFound();

            position.PositionTitle = model.PositionTitle;
            position.PositionCode  = model.PositionCode;
            position.DepartmentID  = model.DepartmentID;
            position.BasicSalary   = model.BasicSalary;
            position.Description   = model.Description;
            position.IsActive      = model.IsActive;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Update", "Position", $"Updated position: {position.PositionTitle}", GetClientIP());

            TempData["Success"] = $"Position '{position.PositionTitle}' updated.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetDepartmentList() =>
            await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem { Value = d.DepartmentID.ToString(), Text = d.DepartmentName })
                .ToListAsync();
    }
}
