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
    [Authorize(Roles = "HR Admin,HR Staff")]
    public class DeductionController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public DeductionController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // ── List ──────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(int? month, int? year, string? cutoff, int? employeeId)
        {
            month ??= DateTime.Today.Month;
            year  ??= DateTime.Today.Year;

            var query = _context.EmployeeDeductions
                .Include(d => d.Employee)
                .Where(d => d.Month == month && d.Year == year);

            if (!string.IsNullOrEmpty(cutoff))
                query = query.Where(d => d.CutoffPeriod == cutoff);

            if (employeeId.HasValue)
                query = query.Where(d => d.EmployeeID == employeeId);

            var deductions = await query
                .OrderBy(d => d.Employee!.LastName)
                .ThenBy(d => d.DeductionType)
                .ToListAsync();

            var vm = deductions.Select(d => new DeductionListViewModel
            {
                DeductionID   = d.DeductionID,
                EmployeeName  = d.Employee?.DisplayName ?? string.Empty,
                EmployeeNo    = d.Employee?.EmployeeNo ?? string.Empty,
                DeductionType = d.DeductionType,
                Description   = d.Description,
                Amount        = d.Amount,
                CutoffPeriod  = d.CutoffPeriod,
                Month         = d.Month,
                Year          = d.Year,
                Status        = d.Status,
                CreatedAt     = d.CreatedAt
            }).ToList();

            ViewBag.Month      = month;
            ViewBag.Year       = year;
            ViewBag.Cutoff     = cutoff;
            ViewBag.EmployeeId = employeeId;
            ViewBag.Employees  = await GetEmployeeSelectList();
            ViewBag.TotalAmount = vm.Where(d => d.Status != "Cancelled").Sum(d => d.Amount);

            return View(vm);
        }

        // ── Create ────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Create()
        {
            var vm = new DeductionViewModel
            {
                Month     = DateTime.Today.Month,
                Year      = DateTime.Today.Year,
                Employees = await GetEmployeeSelectList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeductionViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Employees = await GetEmployeeSelectList();
                return View(vm);
            }

            var deduction = new EmployeeDeduction
            {
                EmployeeID    = vm.EmployeeID,
                DeductionType = vm.DeductionType,
                Description   = vm.Description,
                Amount        = vm.Amount,
                CutoffPeriod  = vm.CutoffPeriod,
                Month         = vm.Month,
                Year          = vm.Year,
                Status        = "Active",
                CreatedAt     = DateTime.Now,
                CreatedBy     = GetCurrentUserID()
            };

            _context.EmployeeDeductions.Add(deduction);
            await _context.SaveChangesAsync();

            var emp = await _context.Employees.FindAsync(vm.EmployeeID);
            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Create", "Deduction",
                $"Added {vm.DeductionType} deduction of ₱{vm.Amount:N2} for {emp?.DisplayName} ({vm.Year}-{vm.Month:D2} {vm.CutoffPeriod})",
                GetClientIP());

            TempData["Success"] = "Deduction added successfully.";
            return RedirectToAction(nameof(Index), new { month = vm.Month, year = vm.Year });
        }

        // ── Edit ──────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var d = await _context.EmployeeDeductions.FindAsync(id);
            if (d == null) return NotFound();

            var vm = new DeductionViewModel
            {
                DeductionID   = d.DeductionID,
                EmployeeID    = d.EmployeeID,
                DeductionType = d.DeductionType,
                Description   = d.Description,
                Amount        = d.Amount,
                CutoffPeriod  = d.CutoffPeriod,
                Month         = d.Month,
                Year          = d.Year,
                Employees     = await GetEmployeeSelectList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DeductionViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Employees = await GetEmployeeSelectList();
                return View(vm);
            }

            var d = await _context.EmployeeDeductions.FindAsync(vm.DeductionID);
            if (d == null) return NotFound();

            d.EmployeeID    = vm.EmployeeID;
            d.DeductionType = vm.DeductionType;
            d.Description   = vm.Description;
            d.Amount        = vm.Amount;
            d.CutoffPeriod  = vm.CutoffPeriod;
            d.Month         = vm.Month;
            d.Year          = vm.Year;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Deduction updated.";
            return RedirectToAction(nameof(Index), new { month = vm.Month, year = vm.Year });
        }

        // ── Cancel ────────────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var d = await _context.EmployeeDeductions.FindAsync(id);
            if (d == null) return NotFound();

            d.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Deduction cancelled.";
            return RedirectToAction(nameof(Index), new { month = d.Month, year = d.Year });
        }

        // ── Helper ────────────────────────────────────────────────────────────────
        private async Task<IEnumerable<SelectListItem>> GetEmployeeSelectList() =>
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
