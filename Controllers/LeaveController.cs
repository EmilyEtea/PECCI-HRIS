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
    [Authorize]
    public class LeaveController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly LeaveCreditService _leaveCreditService;

        public LeaveController(ApplicationDbContext context, AuditService auditService,
            LeaveCreditService leaveCreditService)
        {
            _context = context;
            _auditService = auditService;
            _leaveCreditService = leaveCreditService;
        }

        // ── Leave Applications List ───────────────────────────────────────────────
        public async Task<IActionResult> Index(string? status, int? employeeId)
        {
            var query = _context.LeaveApplications
                .Include(l => l.Employee).ThenInclude(e => e!.Department)
                .Include(l => l.LeaveType)
                .AsQueryable();

            // Employees only see their own
            if (GetCurrentRole() == "Employee")
            {
                int empId = int.Parse(User.FindFirst("EmployeeID")?.Value ?? "0");
                query = query.Where(l => l.EmployeeID == empId);
            }
            else if (employeeId.HasValue)
            {
                query = query.Where(l => l.EmployeeID == employeeId);
            }

            if (!string.IsNullOrEmpty(status))
                query = query.Where(l => l.Status == status);

            var applications = await query
                .OrderByDescending(l => l.AppliedAt)
                .ToListAsync();

            ViewBag.Status     = status;
            ViewBag.EmployeeId = employeeId;
            ViewBag.Employees  = await _context.Employees
                .Where(e => e.Status == "Active").OrderBy(e => e.LastName).ToListAsync();

            return View(applications);
        }

        // ── Apply for Leave ───────────────────────────────────────────────────────
        public async Task<IActionResult> Apply()
        {
            int empId = int.Parse(User.FindFirst("EmployeeID")?.Value ?? "0");

            var leaveCredits = await _context.LeaveCredits
                .Include(lc => lc.LeaveType)
                .Where(lc => lc.EmployeeID == empId && lc.Year == DateTime.Today.Year)
                .ToListAsync();

            var vm = new LeaveApplicationViewModel
            {
                EmployeeID   = empId,
                StartDate    = DateTime.Today,
                EndDate      = DateTime.Today,
                LeaveTypes   = leaveCredits.Select(lc => new SelectListItem
                {
                    Value = lc.LeaveTypeID.ToString(),
                    Text  = $"{lc.LeaveType!.LeaveTypeName} ({lc.RemainingCredits:F1} days remaining)"
                }),
                LeaveCredits = leaveCredits
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(LeaveApplicationViewModel vm)
        {
            int empId = int.Parse(User.FindFirst("EmployeeID")?.Value ?? "0");

            if (vm.StartDate > vm.EndDate)
                ModelState.AddModelError("", "End date must be after start date.");

            // Check leave balance
            var credit = await _context.LeaveCredits
                .FirstOrDefaultAsync(lc => lc.EmployeeID == empId &&
                                           lc.LeaveTypeID == vm.LeaveTypeID &&
                                           lc.Year == DateTime.Today.Year);

            decimal days = (decimal)(vm.EndDate - vm.StartDate).TotalDays + 1;

            if (credit == null || credit.RemainingCredits < days)
                ModelState.AddModelError("", $"Insufficient leave balance. You have {credit?.RemainingCredits ?? 0} day(s) remaining.");

            if (!ModelState.IsValid)
            {
                var leaveCredits = await _context.LeaveCredits
                    .Include(lc => lc.LeaveType)
                    .Where(lc => lc.EmployeeID == empId && lc.Year == DateTime.Today.Year)
                    .ToListAsync();
                vm.LeaveTypes   = leaveCredits.Select(lc => new SelectListItem
                {
                    Value = lc.LeaveTypeID.ToString(),
                    Text  = $"{lc.LeaveType!.LeaveTypeName} ({lc.RemainingCredits:F1} days remaining)"
                });
                vm.LeaveCredits = leaveCredits;
                return View(vm);
            }

            var application = new LeaveApplication
            {
                EmployeeID    = empId,
                LeaveTypeID   = vm.LeaveTypeID,
                StartDate     = vm.StartDate,
                EndDate       = vm.EndDate,
                NumberOfDays  = days,
                Reason        = vm.Reason,
                Status        = "Pending",
                AppliedAt     = DateTime.Now
            };

            _context.LeaveApplications.Add(application);

            // Mark credits as pending
            if (credit != null)
            {
                credit.PendingCredits += days;
                credit.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Apply", "Leave",
                $"Applied for {days} day(s) leave from {vm.StartDate:MM/dd/yyyy} to {vm.EndDate:MM/dd/yyyy}",
                GetClientIP());

            TempData["Success"] = "Leave application submitted successfully. Awaiting approval.";
            return RedirectToAction(nameof(Index));
        }

        // ── Approve / Reject (Manager & HR) ──────────────────────────────────────
        [Authorize(Roles = "HR Admin,HR Staff,Manager")]
        public async Task<IActionResult> Review(int id)
        {
            var application = await _context.LeaveApplications
                .Include(l => l.Employee).ThenInclude(e => e!.Department)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(l => l.LeaveApplicationID == id);

            if (application == null) return NotFound();
            return View(application);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin,HR Staff,Manager")]
        public async Task<IActionResult> Review(LeaveApprovalViewModel vm)
        {
            var application = await _context.LeaveApplications
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.LeaveApplicationID == vm.LeaveApplicationID);

            if (application == null) return NotFound();

            string role = GetCurrentRole();
            bool isApproved = vm.Action == "Approve";

            if (role == "Manager")
            {
                application.ManagerApproverID = GetCurrentUserID();
                application.ManagerApprovedAt = DateTime.Now;
                application.ManagerRemarks    = vm.Remarks;
                application.Status = isApproved ? "Pending HR" : "Rejected";
            }
            else // HR Admin or HR Staff
            {
                application.HRApproverID = GetCurrentUserID();
                application.HRApprovedAt = DateTime.Now;
                application.HRRemarks    = vm.Remarks;
                application.Status = isApproved ? "Approved" : "Rejected";
            }

            application.UpdatedAt = DateTime.Now;

            // If approved by HR, deduct from leave credits
            if (application.Status == "Approved")
            {
                var credit = await _context.LeaveCredits
                    .FirstOrDefaultAsync(lc => lc.EmployeeID == application.EmployeeID &&
                                               lc.LeaveTypeID == application.LeaveTypeID &&
                                               lc.Year == DateTime.Today.Year);
                if (credit != null)
                {
                    credit.UsedCredits    += application.NumberOfDays;
                    credit.PendingCredits -= application.NumberOfDays;
                    if (credit.PendingCredits < 0) credit.PendingCredits = 0;
                    credit.UpdatedAt = DateTime.Now;
                }
            }
            else if (application.Status == "Rejected")
            {
                // Release pending credits
                var credit = await _context.LeaveCredits
                    .FirstOrDefaultAsync(lc => lc.EmployeeID == application.EmployeeID &&
                                               lc.LeaveTypeID == application.LeaveTypeID &&
                                               lc.Year == DateTime.Today.Year);
                if (credit != null)
                {
                    credit.PendingCredits -= application.NumberOfDays;
                    if (credit.PendingCredits < 0) credit.PendingCredits = 0;
                    credit.UpdatedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                vm.Action, "Leave",
                $"{vm.Action}d leave application #{application.LeaveApplicationID} for {application.Employee?.FullName}",
                GetClientIP());

            TempData["Success"] = $"Leave application {vm.Action.ToLower()}d successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── Cancel ────────────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var application = await _context.LeaveApplications.FindAsync(id);
            if (application == null) return NotFound();

            if (application.Status != "Pending" && application.Status != "Pending HR")
            {
                TempData["Error"] = "Only pending applications can be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            // Release pending credits
            var credit = await _context.LeaveCredits
                .FirstOrDefaultAsync(lc => lc.EmployeeID == application.EmployeeID &&
                                           lc.LeaveTypeID == application.LeaveTypeID &&
                                           lc.Year == DateTime.Today.Year);
            if (credit != null)
            {
                credit.PendingCredits -= application.NumberOfDays;
                if (credit.PendingCredits < 0) credit.PendingCredits = 0;
                credit.UpdatedAt = DateTime.Now;
            }

            application.Status    = "Cancelled";
            application.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave application cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // ── Leave Credits ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Credits(int? employeeId)
        {
            int empId = GetCurrentRole() == "Employee"
                ? int.Parse(User.FindFirst("EmployeeID")?.Value ?? "0")
                : employeeId ?? 0;

            var credits = await _context.LeaveCredits
                .Include(lc => lc.Employee)
                .Include(lc => lc.LeaveType)
                .Where(lc => (empId == 0 || lc.EmployeeID == empId) && lc.Year == DateTime.Today.Year)
                .OrderBy(lc => lc.Employee!.LastName)
                .ThenBy(lc => lc.LeaveType!.LeaveTypeName)
                .ToListAsync();

            ViewBag.Employees  = await _context.Employees
                .Where(e => e.Status == "Active").OrderBy(e => e.LastName).ToListAsync();
            ViewBag.EmployeeId = empId;

            return View(credits);
        }

        // ── Leave Types (HR Admin only) ───────────────────────────────────────────
        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Types()
        {
            var types = await _context.LeaveTypes.OrderBy(lt => lt.LeaveTypeName).ToListAsync();
            return View(types);
        }

        [Authorize(Roles = "HR Admin")]
        public IActionResult CreateType() => View(new LeaveTypeViewModel());

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> CreateType(LeaveTypeViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var leaveType = new LeaveType
            {
                LeaveTypeName      = vm.LeaveTypeName,
                LeaveCode          = vm.LeaveCode,
                Description        = vm.Description,
                DefaultDaysPerYear = vm.DefaultDaysPerYear,
                IsPaid             = vm.IsPaid,
                RequiresApproval   = vm.RequiresApproval,
                IsActive           = vm.IsActive,
                CreatedAt          = DateTime.Now
            };

            _context.LeaveTypes.Add(leaveType);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Leave type '{leaveType.LeaveTypeName}' created.";
            return RedirectToAction(nameof(Types));
        }

        // ── Manual Annual Refresh (HR Admin only) ─────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> RefreshCredits(int year)
        {
            int count = await _leaveCreditService.ManualRefresh(year);

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "RefreshCredits", "Leave",
                $"Manually refreshed leave credits for year {year} — {count} records updated",
                GetClientIP());

            TempData["Success"] = $"Leave credits refreshed for {year}. {count} records updated.";
            return RedirectToAction(nameof(Credits));
        }
    }
}
