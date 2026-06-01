using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;
using PECCI_HRIS.Models;
using PECCI_HRIS.Services;
using PECCI_HRIS.ViewModels;

namespace PECCI_HRIS.Controllers
{
    [Authorize]
    public class OvertimeController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly EmailService _emailService;

        public OvertimeController(ApplicationDbContext context,
            AuditService auditService, EmailService emailService)
        {
            _context = context;
            _auditService = auditService;
            _emailService = emailService;
        }

        // ── List ──────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? status, int? employeeId)
        {
            var query = _context.OvertimeRequests
                .Include(o => o.Employee).ThenInclude(e => e!.Department)
                .AsQueryable();

            string role = GetCurrentRole();

            if (role == "Employee")
            {
                int empId = int.Parse(User.FindFirst("EmployeeID")?.Value ?? "0");
                query = query.Where(o => o.EmployeeID == empId);
            }
            else if (employeeId.HasValue)
            {
                query = query.Where(o => o.EmployeeID == employeeId);
            }

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            var requests = await query
                .OrderByDescending(o => o.OvertimeDate)
                .ThenByDescending(o => o.AppliedAt)
                .ToListAsync();

            ViewBag.Status     = status;
            ViewBag.EmployeeId = employeeId;
            ViewBag.IsHR       = role == "HR Admin" || role == "HR Staff";
            ViewBag.IsManager  = role == "Manager";
            ViewBag.Employees  = await _context.Employees
                .Where(e => e.Status == "Active").OrderBy(e => e.LastName).ToListAsync();

            return View(requests);
        }

        // ── Submit ────────────────────────────────────────────────────────────

        public IActionResult Create()
        {
            int empId = int.Parse(User.FindFirst("EmployeeID")?.Value ?? "0");
            return View(new OvertimeRequestViewModel
            {
                EmployeeID       = empId,
                OvertimeDate     = DateTime.Today,
                StartTimeString  = "17:00",
                EndTimeString    = "19:00"
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OvertimeRequestViewModel vm)
        {
            int empId = int.Parse(User.FindFirst("EmployeeID")?.Value ?? "0");

            // Parse times from string fields
            if (!TimeSpan.TryParse(vm.StartTimeString, out _))
                ModelState.AddModelError("StartTimeString", "Invalid start time.");
            if (!TimeSpan.TryParse(vm.EndTimeString, out _))
                ModelState.AddModelError("EndTimeString", "Invalid end time.");

            if (ModelState.IsValid && vm.EndTime <= vm.StartTime)
                ModelState.AddModelError("EndTimeString", "End time must be after start time.");

            double requestedMinutes = ModelState.IsValid
                ? (vm.EndTime - vm.StartTime).TotalMinutes : 0;

            if (ModelState.IsValid && requestedMinutes < 30)
                ModelState.AddModelError("", "Minimum overtime request is 30 minutes.");

            // Prevent duplicate request for the same date
            bool duplicate = await _context.OvertimeRequests.AnyAsync(o =>
                o.EmployeeID == empId &&
                o.OvertimeDate == vm.OvertimeDate.Date &&
                o.Status != "Cancelled" && o.Status != "Disapproved");
            if (duplicate)
                ModelState.AddModelError("", "You already have an active OT request for this date.");

            if (!ModelState.IsValid) return View(vm);

            var request = new OvertimeRequest
            {
                EmployeeID        = empId,
                OvertimeDate      = vm.OvertimeDate.Date,
                StartTime         = vm.StartTime,
                EndTime           = vm.EndTime,
                RequestedMinutes  = requestedMinutes,
                Reason            = vm.Reason,
                Status            = "Pending",
                AppliedAt         = DateTime.Now
            };

            _context.OvertimeRequests.Add(request);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Create", "OvertimeRequest",
                $"Submitted OT request for {vm.OvertimeDate:yyyy-MM-dd} " +
                $"({vm.StartTime:hh\\:mm}–{vm.EndTime:hh\\:mm}, {requestedMinutes:F0} min)",
                GetClientIP());

            // Notify HR/Manager
            var employee = await _context.Employees
                .Include(e => e.UserAccount)
                .FirstOrDefaultAsync(e => e.EmployeeID == empId);

            var reviewers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive &&
                    (u.Role!.RoleName == "HR Admin" || u.Role.RoleName == "HR Staff" ||
                     u.Role.RoleName == "Manager"))
                .ToListAsync();

            foreach (var reviewer in reviewers)
            {
                _ = SendOtPendingReviewEmail(
                    reviewer.Email, reviewer.Username,
                    employee?.DisplayName ?? "An employee",
                    request.OvertimeDate, request.StartTime, request.EndTime,
                    requestedMinutes, request.OvertimeRequestID);
            }

            TempData["Success"] = "Overtime request submitted. Awaiting approval.";
            return RedirectToAction(nameof(Index));
        }

        // ── Details ───────────────────────────────────────────────────────────

        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.OvertimeRequests
                .Include(o => o.Employee).ThenInclude(e => e!.Department)
                .Include(o => o.Employee!.Position)
                .FirstOrDefaultAsync(o => o.OvertimeRequestID == id);

            if (request == null) return NotFound();

            if (GetCurrentRole() == "Employee")
            {
                int empId = int.Parse(User.FindFirst("EmployeeID")?.Value ?? "0");
                if (request.EmployeeID != empId) return Forbid();
            }

            return View(request);
        }

        // ── Review ────────────────────────────────────────────────────────────

        [Authorize(Roles = "HR Admin,HR Staff,Manager")]
        public async Task<IActionResult> Review(int id)
        {
            var request = await _context.OvertimeRequests
                .Include(o => o.Employee).ThenInclude(e => e!.Department)
                .FirstOrDefaultAsync(o => o.OvertimeRequestID == id);

            if (request == null) return NotFound();

            ViewBag.DefaultApprovedMinutes = request.RequestedMinutes;
            return View(request);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin,HR Staff,Manager")]
        public async Task<IActionResult> Review(OvertimeReviewViewModel vm)
        {
            var request = await _context.OvertimeRequests
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(o => o.OvertimeRequestID == vm.OvertimeRequestID);

            if (request == null) return NotFound();

            string role       = GetCurrentRole();
            bool   isApproved = vm.Action == "Approve";

            if (role == "Manager")
            {
                request.ManagerApproverID = GetCurrentUserID();
                request.ManagerApprovedAt = DateTime.Now;
                request.ManagerRemarks    = vm.Remarks;
                request.Status = isApproved ? "Pending HR" : "Disapproved";
            }
            else // HR Admin or HR Staff
            {
                request.HRApproverID = GetCurrentUserID();
                request.HRApprovedAt = DateTime.Now;
                request.HRRemarks    = vm.Remarks;
                request.Status = isApproved ? "Approved" : "Disapproved";

                if (isApproved)
                {
                    // HR can cap the approved minutes; default to requested if not specified
                    request.ApprovedMinutes = vm.ApprovedMinutes ?? request.RequestedMinutes;
                }
            }

            request.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                vm.Action, "OvertimeRequest",
                $"{vm.Action}d OT request #{request.OvertimeRequestID} for " +
                $"{request.Employee?.FullName} on {request.OvertimeDate:yyyy-MM-dd}",
                GetClientIP());

            // Email employee
            var empUser = await _context.Users
                .FirstOrDefaultAsync(u => u.EmployeeID == request.EmployeeID);

            if (empUser?.Email != null &&
                (request.Status == "Approved" || request.Status == "Disapproved"))
            {
                _ = SendOtDecisionEmail(
                    empUser.Email,
                    request.Employee?.DisplayName ?? empUser.Username,
                    request.OvertimeDate, request.StartTime, request.EndTime,
                    request.ApprovedMinutes ?? request.RequestedMinutes,
                    request.Status, GetCurrentUsername(), vm.Remarks);
            }

            // If Manager approved, alert HR for final review
            if (request.Status == "Pending HR")
            {
                var hrUsers = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.IsActive &&
                        (u.Role!.RoleName == "HR Admin" || u.Role.RoleName == "HR Staff"))
                    .ToListAsync();

                foreach (var hr in hrUsers)
                {
                    _ = SendOtPendingReviewEmail(
                        hr.Email, hr.Username,
                        request.Employee?.DisplayName ?? "An employee",
                        request.OvertimeDate, request.StartTime, request.EndTime,
                        request.RequestedMinutes, request.OvertimeRequestID);
                }
            }

            TempData["Success"] = $"OT request {vm.Action.ToLower()}d successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── Cancel ────────────────────────────────────────────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var request = await _context.OvertimeRequests.FindAsync(id);
            if (request == null) return NotFound();

            if (request.Status != "Pending" && request.Status != "Pending HR")
            {
                TempData["Error"] = "Only pending requests can be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            // Employees can only cancel their own
            if (GetCurrentRole() == "Employee")
            {
                int empId = int.Parse(User.FindFirst("EmployeeID")?.Value ?? "0");
                if (request.EmployeeID != empId) return Forbid();
            }

            request.Status    = "Cancelled";
            request.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Cancel", "OvertimeRequest",
                $"Cancelled OT request #{request.OvertimeRequestID}",
                GetClientIP());

            TempData["Success"] = "Overtime request cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // ── Email helpers ─────────────────────────────────────────────────────

        private Task SendOtPendingReviewEmail(
            string toEmail, string toName, string employeeName,
            DateTime date, TimeSpan start, TimeSpan end, double minutes, int requestId)
        {
            string subject = $"[PECCI HRIS] OT Request Pending Review — {employeeName}";
            string body = $@"
{OtEmailHeader()}
<p>Hi <strong>{HtmlEncode(toName)}</strong>,</p>
<p>An overtime request is pending your review.</p>
{OtDetailsTable(employeeName, date, start, end, minutes, "Pending Review", "#fd7e14")}
<p>
  <a href=""/Overtime/Review/{requestId}""
     style=""display:inline-block;padding:10px 20px;background:#2d6a4f;color:#fff;
             text-decoration:none;border-radius:4px;font-weight:600;"">
    Review Request
  </a>
</p>
{OtEmailFooter()}";
            return _emailService.SendAsync(toEmail, toName, subject, body);
        }

        private Task SendOtDecisionEmail(
            string toEmail, string toName,
            DateTime date, TimeSpan start, TimeSpan end, double approvedMinutes,
            string status, string approverName, string? remarks)
        {
            bool approved = status == "Approved";
            string subject = approved
                ? "[PECCI HRIS] Overtime Request Approved"
                : "[PECCI HRIS] Overtime Request Disapproved";

            string remarksHtml = string.IsNullOrWhiteSpace(remarks)
                ? ""
                : $"<p><strong>Remarks:</strong> {HtmlEncode(remarks)}</p>";

            string body = $@"
{OtEmailHeader()}
<p>Hi <strong>{HtmlEncode(toName)}</strong>,</p>
<p>Your overtime request has been <strong>{(approved ? "approved" : "disapproved")}</strong>.</p>
{OtDetailsTable(toName, date, start, end, approvedMinutes,
    approved ? "✅ Approved" : "❌ Disapproved",
    approved ? "#2d6a4f" : "#dc3545")}
<p><strong>Reviewed by:</strong> {HtmlEncode(approverName)}</p>
{remarksHtml}
{OtEmailFooter()}";
            return _emailService.SendAsync(toEmail, toName, subject, body);
        }

        private static string OtEmailHeader() => @"
<!DOCTYPE html><html><body style=""font-family:Arial,sans-serif;color:#212529;max-width:600px;margin:0 auto;padding:20px;"">
<div style=""background:#2d6a4f;padding:16px 24px;border-radius:6px 6px 0 0;"">
  <h2 style=""color:#fff;margin:0;"">PECCI HRIS</h2>
  <p style=""color:#b7e4c7;margin:4px 0 0;"">Overtime Request Notification</p>
</div>
<div style=""border:1px solid #dee2e6;border-top:none;padding:24px;border-radius:0 0 6px 6px;"">";

        private static string OtEmailFooter() => @"
<hr style=""border:none;border-top:1px solid #dee2e6;margin:24px 0;""/>
<p style=""color:#6c757d;font-size:0.8em;margin:0;"">This is an automated message from PECCI HRIS.</p>
</div></body></html>";

        private static string OtDetailsTable(
            string employeeName, DateTime date, TimeSpan start, TimeSpan end,
            double minutes, string status, string statusColor)
        {
            double hours = minutes / 60.0;
            return $@"
<table style=""width:100%;border-collapse:collapse;margin:16px 0;border:1px solid #dee2e6;"">
  <tbody>
    <tr style=""background:#f8f9fa;"">
      <td style=""padding:6px 12px;color:#6c757d;"">Employee</td>
      <td style=""padding:6px 12px;""><strong>{HtmlEncode(employeeName)}</strong></td>
    </tr>
    <tr>
      <td style=""padding:6px 12px;color:#6c757d;"">Date</td>
      <td style=""padding:6px 12px;"">{date:MMMM d, yyyy} ({date:dddd})</td>
    </tr>
    <tr style=""background:#f8f9fa;"">
      <td style=""padding:6px 12px;color:#6c757d;"">Time</td>
      <td style=""padding:6px 12px;"">{start:hh\\:mm} – {end:hh\\:mm}</td>
    </tr>
    <tr>
      <td style=""padding:6px 12px;color:#6c757d;"">Duration</td>
      <td style=""padding:6px 12px;"">{hours:F2} hour(s) ({minutes:F0} min)</td>
    </tr>
    <tr style=""background:#f8f9fa;"">
      <td style=""padding:6px 12px;color:#6c757d;"">Status</td>
      <td style=""padding:6px 12px;"">
        <span style=""background:{statusColor};color:#fff;padding:2px 10px;
                      border-radius:12px;font-size:0.85em;font-weight:600;"">
          {HtmlEncode(status)}
        </span>
      </td>
    </tr>
  </tbody>
</table>";
        }

        private static string HtmlEncode(string? s)
            => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
    }
}
