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
    public class RecurringDeductionController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly RecurringDeductionService _recurringService;
        private readonly AuditService _auditService;

        public RecurringDeductionController(ApplicationDbContext context,
            RecurringDeductionService recurringService,
            AuditService auditService)
        {
            _context          = context;
            _recurringService = recurringService;
            _auditService     = auditService;
        }

        // ── List ──────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(string? status, int? employeeId)
        {
            var query = _context.RecurringDeductionSchedules
                .Include(s => s.Employee)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(s => s.Status == status);

            if (employeeId.HasValue)
                query = query.Where(s => s.EmployeeID == employeeId);

            var schedules = await query
                .OrderBy(s => s.Employee!.LastName)
                .ThenBy(s => s.DeductionType)
                .ToListAsync();

            var vm = schedules.Select(s => new RecurringDeductionListViewModel
            {
                ScheduleID          = s.ScheduleID,
                EmployeeName        = s.Employee?.DisplayName ?? string.Empty,
                EmployeeNo          = s.Employee?.EmployeeNo ?? string.Empty,
                DeductionType       = s.DeductionType,
                Description         = s.Description,
                AmountPerCutoff     = s.AmountPerCutoff,
                CutoffPeriod        = s.CutoffPeriod,
                StartDate           = s.StartDate,
                EndDate             = s.EndDate,
                TotalInstallments   = s.TotalInstallments,
                AppliedInstallments = s.AppliedInstallments,
                Status              = s.Status
            }).ToList();

            ViewBag.Status     = status;
            ViewBag.EmployeeId = employeeId;
            ViewBag.Employees  = await GetEmployeeSelectList();

            return View(vm);
        }

        // ── Create ────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Create()
        {
            var vm = new RecurringDeductionViewModel
            {
                StartDate = DateTime.Today,
                Employees = await GetEmployeeSelectList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecurringDeductionViewModel vm)
        {
            if (vm.EndDate.HasValue && vm.EndDate < vm.StartDate)
                ModelState.AddModelError("EndDate", "End date cannot be before start date.");

            if (!ModelState.IsValid)
            {
                vm.Employees = await GetEmployeeSelectList();
                return View(vm);
            }

            var schedule = new RecurringDeductionSchedule
            {
                EmployeeID         = vm.EmployeeID,
                DeductionType      = vm.DeductionType,
                Description        = vm.Description,
                AmountPerCutoff    = vm.AmountPerCutoff,
                CutoffPeriod       = vm.CutoffPeriod,
                StartDate          = vm.StartDate,
                EndDate            = vm.EndDate,
                TotalInstallments  = vm.TotalInstallments,
                AppliedInstallments = 0,
                Status             = "Active",
                CreatedAt          = DateTime.Now,
                CreatedBy          = GetCurrentUserID()
            };

            _context.RecurringDeductionSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            var emp = await _context.Employees.FindAsync(vm.EmployeeID);
            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Create", "RecurringDeduction",
                $"Created recurring {vm.DeductionType} of ₱{vm.AmountPerCutoff:N2}/cutoff for {emp?.DisplayName}",
                GetClientIP());

            TempData["Success"] = "Recurring deduction schedule created.";
            return RedirectToAction(nameof(Index));
        }

        // ── Edit ──────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var s = await _context.RecurringDeductionSchedules.FindAsync(id);
            if (s == null) return NotFound();

            var vm = new RecurringDeductionViewModel
            {
                ScheduleID        = s.ScheduleID,
                EmployeeID        = s.EmployeeID,
                DeductionType     = s.DeductionType,
                Description       = s.Description,
                AmountPerCutoff   = s.AmountPerCutoff,
                CutoffPeriod      = s.CutoffPeriod,
                StartDate         = s.StartDate,
                EndDate           = s.EndDate,
                TotalInstallments = s.TotalInstallments,
                Employees         = await GetEmployeeSelectList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RecurringDeductionViewModel vm)
        {
            if (vm.EndDate.HasValue && vm.EndDate < vm.StartDate)
                ModelState.AddModelError("EndDate", "End date cannot be before start date.");

            if (!ModelState.IsValid)
            {
                vm.Employees = await GetEmployeeSelectList();
                return View(vm);
            }

            var s = await _context.RecurringDeductionSchedules.FindAsync(vm.ScheduleID);
            if (s == null) return NotFound();

            s.DeductionType     = vm.DeductionType;
            s.Description       = vm.Description;
            s.AmountPerCutoff   = vm.AmountPerCutoff;
            s.CutoffPeriod      = vm.CutoffPeriod;
            s.StartDate         = vm.StartDate;
            s.EndDate           = vm.EndDate;
            s.TotalInstallments = vm.TotalInstallments;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Schedule updated.";
            return RedirectToAction(nameof(Index));
        }

        // ── Pause / Resume ────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePause(int id)
        {
            var s = await _context.RecurringDeductionSchedules.FindAsync(id);
            if (s == null) return NotFound();

            s.Status = s.Status == "Active" ? "Paused" : "Active";
            await _context.SaveChangesAsync();

            TempData["Success"] = s.Status == "Paused" ? "Schedule paused." : "Schedule resumed.";
            return RedirectToAction(nameof(Index));
        }

        // ── Cancel ────────────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var s = await _context.RecurringDeductionSchedules.FindAsync(id);
            if (s == null) return NotFound();

            s.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Schedule cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // ── Generate for cutoff ───────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int month, int year, string cutoffPeriod)
        {
            int count = await _recurringService.GenerateForCutoff(year, month, cutoffPeriod);

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Generate", "RecurringDeduction",
                $"Generated {count} recurring deduction(s) for {year}-{month:D2} {cutoffPeriod}",
                GetClientIP());

            TempData["Success"] = count > 0
                ? $"{count} recurring deduction(s) generated for {year}-{month:D2} {cutoffPeriod}."
                : "No new deductions to generate — all schedules already applied for this cutoff.";

            return RedirectToAction("Index", "Deduction", new { month, year, cutoff = cutoffPeriod });
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
