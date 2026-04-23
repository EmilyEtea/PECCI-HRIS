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
    public class EmployeeController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public EmployeeController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // ── List ─────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(string? search, string? status, int? departmentId)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.FirstName.Contains(search) ||
                                         e.LastName.Contains(search) ||
                                         e.EmployeeNo.Contains(search));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status == status);

            if (departmentId.HasValue)
                query = query.Where(e => e.DepartmentID == departmentId);

            var employees = await query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync();

            ViewBag.Search       = search;
            ViewBag.Status       = status;
            ViewBag.DepartmentId = departmentId;
            ViewBag.Departments  = await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.DepartmentName).ToListAsync();

            return View(employees);
        }

        // ── Profile / Details ─────────────────────────────────────────────────────
        public async Task<IActionResult> Profile(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.UserAccount)
                .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee == null) return NotFound();

            // Leave credits
            var leaveCredits = await _context.LeaveCredits
                .Include(lc => lc.LeaveType)
                .Where(lc => lc.EmployeeID == id && lc.Year == DateTime.Today.Year)
                .ToListAsync();

            // Recent attendance
            var recentAttendance = await _context.AttendanceRecords
                .Where(a => a.EmployeeID == id)
                .OrderByDescending(a => a.AttendanceDate)
                .Take(10)
                .ToListAsync();

            ViewBag.LeaveCredits     = leaveCredits;
            ViewBag.RecentAttendance = recentAttendance;

            return View(employee);
        }

        // ── Create ────────────────────────────────────────────────────────────────
        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Create()
        {
            var vm = new EmployeeViewModel
            {
                DateHired    = DateTime.Today,
                DateOfBirth  = DateTime.Today.AddYears(-25),
                Departments  = await GetDepartmentList(),
                Positions    = new List<SelectListItem>()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Create(EmployeeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Departments = await GetDepartmentList();
                vm.Positions   = await GetPositionList(vm.DepartmentID);
                return View(vm);
            }

            var employee = new Employee
            {
                EmployeeNo       = vm.EmployeeNo,
                FirstName        = vm.FirstName,
                MiddleName       = vm.MiddleName,
                LastName         = vm.LastName,
                Suffix           = vm.Suffix,
                DateOfBirth      = vm.DateOfBirth,
                Gender           = vm.Gender,
                CivilStatus      = vm.CivilStatus,
                Nationality      = vm.Nationality ?? "Filipino",
                Address          = vm.Address,
                ContactNumber    = vm.ContactNumber,
                PersonalEmail    = vm.PersonalEmail,
                CompanyEmail     = vm.CompanyEmail,
                SSSNumber        = vm.SSSNumber,
                PhilHealthNumber = vm.PhilHealthNumber,
                PagIbigNumber    = vm.PagIbigNumber,
                TINNumber        = vm.TINNumber,
                DepartmentID     = vm.DepartmentID,
                PositionID       = vm.PositionID,
                DateHired        = vm.DateHired,
                DateRegularized  = vm.DateRegularized,
                EmploymentStatus = vm.EmploymentStatus,
                Status           = vm.Status,
                CreatedAt        = DateTime.Now,
                CreatedBy        = GetCurrentUserID()
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Auto-allocate leave credits for current year
            await AllocateLeaveCredits(employee.EmployeeID);

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Create", "Employee",
                $"Created employee {employee.FullName} ({employee.EmployeeNo})",
                GetClientIP());

            TempData["Success"] = $"Employee {employee.FullName} created successfully.";
            return RedirectToAction(nameof(Profile), new { id = employee.EmployeeID });
        }

        // ── Edit ──────────────────────────────────────────────────────────────────
        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            var vm = new EmployeeViewModel
            {
                EmployeeID       = employee.EmployeeID,
                EmployeeNo       = employee.EmployeeNo,
                FirstName        = employee.FirstName,
                MiddleName       = employee.MiddleName,
                LastName         = employee.LastName,
                Suffix           = employee.Suffix,
                DateOfBirth      = employee.DateOfBirth,
                Gender           = employee.Gender,
                CivilStatus      = employee.CivilStatus,
                Nationality      = employee.Nationality,
                Address          = employee.Address,
                ContactNumber    = employee.ContactNumber,
                PersonalEmail    = employee.PersonalEmail,
                CompanyEmail     = employee.CompanyEmail,
                SSSNumber        = employee.SSSNumber,
                PhilHealthNumber = employee.PhilHealthNumber,
                PagIbigNumber    = employee.PagIbigNumber,
                TINNumber        = employee.TINNumber,
                DepartmentID     = employee.DepartmentID,
                PositionID       = employee.PositionID,
                DateHired        = employee.DateHired,
                DateRegularized  = employee.DateRegularized,
                EmploymentStatus = employee.EmploymentStatus,
                Status           = employee.Status,
                Departments      = await GetDepartmentList(),
                Positions        = await GetPositionList(employee.DepartmentID)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Edit(EmployeeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Departments = await GetDepartmentList();
                vm.Positions   = await GetPositionList(vm.DepartmentID);
                return View(vm);
            }

            var employee = await _context.Employees.FindAsync(vm.EmployeeID);
            if (employee == null) return NotFound();

            string oldValues = $"Name:{employee.FullName}, Dept:{employee.DepartmentID}, Status:{employee.Status}";

            employee.FirstName        = vm.FirstName;
            employee.MiddleName       = vm.MiddleName;
            employee.LastName         = vm.LastName;
            employee.Suffix           = vm.Suffix;
            employee.DateOfBirth      = vm.DateOfBirth;
            employee.Gender           = vm.Gender;
            employee.CivilStatus      = vm.CivilStatus;
            employee.Nationality      = vm.Nationality;
            employee.Address          = vm.Address;
            employee.ContactNumber    = vm.ContactNumber;
            employee.PersonalEmail    = vm.PersonalEmail;
            employee.CompanyEmail     = vm.CompanyEmail;
            employee.SSSNumber        = vm.SSSNumber;
            employee.PhilHealthNumber = vm.PhilHealthNumber;
            employee.PagIbigNumber    = vm.PagIbigNumber;
            employee.TINNumber        = vm.TINNumber;
            employee.DepartmentID     = vm.DepartmentID;
            employee.PositionID       = vm.PositionID;
            employee.DateHired        = vm.DateHired;
            employee.DateRegularized  = vm.DateRegularized;
            employee.EmploymentStatus = vm.EmploymentStatus;
            employee.Status           = vm.Status;
            employee.UpdatedAt        = DateTime.Now;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Update", "Employee",
                $"Updated employee {employee.FullName} ({employee.EmployeeNo})",
                GetClientIP(), oldValues);

            TempData["Success"] = $"Employee {employee.FullName} updated successfully.";
            return RedirectToAction(nameof(Profile), new { id = employee.EmployeeID });
        }

        // ── Deactivate ────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> Deactivate(int id, string reason)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            employee.Status        = "Inactive";
            employee.DateSeparated = DateTime.Today;
            employee.UpdatedAt     = DateTime.Now;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Deactivate", "Employee",
                $"Deactivated employee {employee.FullName}. Reason: {reason}",
                GetClientIP());

            TempData["Success"] = $"{employee.FullName} has been deactivated.";
            return RedirectToAction(nameof(Index));
        }

        // ── AJAX: Get positions by department ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetPositionsByDepartment(int departmentId)
        {
            var positions = await _context.Positions
                .Where(p => p.DepartmentID == departmentId && p.IsActive)
                .OrderBy(p => p.PositionTitle)
                .Select(p => new { p.PositionID, p.PositionTitle, p.BasicSalary })
                .ToListAsync();

            return Json(positions);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private async Task<IEnumerable<SelectListItem>> GetDepartmentList()
        {
            return await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem { Value = d.DepartmentID.ToString(), Text = d.DepartmentName })
                .ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetPositionList(int departmentId)
        {
            return await _context.Positions
                .Where(p => p.DepartmentID == departmentId && p.IsActive)
                .OrderBy(p => p.PositionTitle)
                .Select(p => new SelectListItem { Value = p.PositionID.ToString(), Text = p.PositionTitle })
                .ToListAsync();
        }

        private async Task AllocateLeaveCredits(int employeeId)
        {
            var leaveTypes = await _context.LeaveTypes.Where(lt => lt.IsActive).ToListAsync();
            int year = DateTime.Today.Year;

            foreach (var lt in leaveTypes)
            {
                bool exists = await _context.LeaveCredits
                    .AnyAsync(lc => lc.EmployeeID == employeeId && lc.LeaveTypeID == lt.LeaveTypeID && lc.Year == year);

                if (!exists)
                {
                    _context.LeaveCredits.Add(new LeaveCredit
                    {
                        EmployeeID   = employeeId,
                        LeaveTypeID  = lt.LeaveTypeID,
                        Year         = year,
                        TotalCredits = lt.DefaultDaysPerYear,
                        UsedCredits  = 0,
                        PendingCredits = 0,
                        CreatedAt    = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
