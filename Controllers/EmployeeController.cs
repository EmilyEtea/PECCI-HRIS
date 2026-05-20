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
        private readonly LeaveCreditService _leaveCreditService;

        public EmployeeController(ApplicationDbContext context,
            AuditService auditService,
            LeaveCreditService leaveCreditService)
        {
            _context = context;
            _auditService = auditService;
            _leaveCreditService = leaveCreditService;
        }

        public async Task<IActionResult> Index(string? search, string? status, int? departmentId)
        {
            // Employees should not see the full employee list — redirect to own profile
            if (GetCurrentRole() == "Employee")
            {
                int empId = int.Parse(User.FindFirst("EmployeeID")?.Value ?? "0");
                if (empId > 0) return RedirectToAction(nameof(Profile), new { id = empId });
                return RedirectToAction("Index", "Dashboard");
            }
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

            var employees = await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.DepartmentId = departmentId;
            ViewBag.Departments = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            return View(employees);
        }

        public async Task<IActionResult> Profile(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.UserAccount)
                .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee == null) return NotFound();

            var leaveCredits = await _context.LeaveCredits
                .Include(lc => lc.LeaveType)
                .Where(lc => lc.EmployeeID == id && lc.Year == DateTime.Today.Year)
                .ToListAsync();

            var recentAttendance = await _context.AttendanceRecords
                .Where(a => a.EmployeeID == id)
                .OrderByDescending(a => a.AttendanceDate)
                .Take(10)
                .ToListAsync();

            ViewBag.LeaveCredits = leaveCredits;
            ViewBag.RecentAttendance = recentAttendance;

            return View(employee);
        }

        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Create()
        {
            var vm = new EmployeeViewModel
            {
                DateHired = DateTime.Today,
                DateOfBirth = DateTime.Today.AddYears(-25),
                Departments = await GetDepartmentList(),
                Positions = new List<SelectListItem>()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Create(EmployeeViewModel vm)
        {
            // Business logic validation
            if (vm.DateOfBirth >= DateTime.Today)
                ModelState.AddModelError("DateOfBirth", "Date of birth cannot be today or in the future.");
            else if ((DateTime.Today - vm.DateOfBirth).TotalDays / 365.25 < 18)
                ModelState.AddModelError("DateOfBirth", "Employee must be at least 18 years old.");

            if (vm.DateHired < vm.DateOfBirth)
                ModelState.AddModelError("DateHired", "Date hired cannot be before date of birth.");

            if (vm.DateRegularized.HasValue && vm.DateRegularized < vm.DateHired)
                ModelState.AddModelError("DateRegularized", "Date regularized cannot be before date hired.");

            bool duplicateNo = await _context.Employees.AnyAsync(e => e.EmployeeNo == vm.EmployeeNo);
            if (duplicateNo)
                ModelState.AddModelError("EmployeeNo", "This Employee No. is already in use.");

            if (!ModelState.IsValid)
            {
                vm.Departments = await GetDepartmentList();
                vm.Positions = await GetPositionList(vm.DepartmentID);
                return View(vm);
            }

            var employee = new Employee
            {
                EmployeeNo = vm.EmployeeNo!,
                FirstName = vm.FirstName,
                MiddleName = vm.MiddleName,
                LastName = vm.LastName,
                Suffix = vm.Suffix,
                DateOfBirth = vm.DateOfBirth,
                Gender = vm.Gender,
                CivilStatus = vm.CivilStatus,
                Nationality = vm.Nationality ?? "Filipino",
                Address = vm.Address,
                ContactNumber = vm.ContactNumber,
                PersonalEmail = vm.PersonalEmail,
                CompanyEmail = vm.CompanyEmail,
                SSSNumber = vm.SSSNumber,
                PhilHealthNumber = vm.PhilHealthNumber,
                PagIbigNumber = vm.PagIbigNumber,
                TINNumber = vm.TINNumber,
                DepartmentID = vm.DepartmentID,
                PositionID = vm.PositionID,
                DateHired = vm.DateHired,
                DateRegularized = vm.DateRegularized,
                EmploymentStatus = vm.EmploymentStatus,
                Status = vm.Status,
                CreatedAt = DateTime.Now,
                CreatedBy = GetCurrentUserID()
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            await _leaveCreditService.AllocateForNewEmployee(employee.EmployeeID);

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Create", "Employee",
                $"Created employee {employee.FullName} ({employee.EmployeeNo})",
                GetClientIP());

            TempData["Success"] = $"Employee {employee.FullName} created successfully.";
            return RedirectToAction(nameof(Profile), new { id = employee.EmployeeID });
        }

        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            var vm = new EmployeeViewModel
            {
                EmployeeID = employee.EmployeeID,
                EmployeeNo = employee.EmployeeNo,
                FirstName = employee.FirstName,
                MiddleName = employee.MiddleName,
                LastName = employee.LastName,
                Suffix = employee.Suffix,
                DateOfBirth = employee.DateOfBirth,
                Gender = employee.Gender,
                CivilStatus = employee.CivilStatus,
                Nationality = employee.Nationality,
                Address = employee.Address,
                ContactNumber = employee.ContactNumber,
                PersonalEmail = employee.PersonalEmail,
                CompanyEmail = employee.CompanyEmail,
                SSSNumber = employee.SSSNumber,
                PhilHealthNumber = employee.PhilHealthNumber,
                PagIbigNumber = employee.PagIbigNumber,
                TINNumber = employee.TINNumber,
                DepartmentID = employee.DepartmentID,
                PositionID = employee.PositionID,
                DateHired = employee.DateHired,
                DateRegularized = employee.DateRegularized,
                EmploymentStatus = employee.EmploymentStatus,
                Status = employee.Status,
                Departments = await GetDepartmentList(),
                Positions = await GetPositionList(employee.DepartmentID)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin,HR Staff")]
        public async Task<IActionResult> Edit(EmployeeViewModel vm)
        {
            // Business logic validation
            if (vm.DateOfBirth >= DateTime.Today)
                ModelState.AddModelError("DateOfBirth", "Date of birth cannot be today or in the future.");
            else if ((DateTime.Today - vm.DateOfBirth).TotalDays / 365.25 < 18)
                ModelState.AddModelError("DateOfBirth", "Employee must be at least 18 years old.");

            if (vm.DateHired < vm.DateOfBirth)
                ModelState.AddModelError("DateHired", "Date hired cannot be before date of birth.");

            if (vm.DateRegularized.HasValue && vm.DateRegularized < vm.DateHired)
                ModelState.AddModelError("DateRegularized", "Date regularized cannot be before date hired.");

            if (!ModelState.IsValid)
            {
                vm.Departments = await GetDepartmentList();
                vm.Positions = await GetPositionList(vm.DepartmentID);
                return View(vm);
            }

            var employee = await _context.Employees.FindAsync(vm.EmployeeID);
            if (employee == null) return NotFound();

            string oldValues = $"Name:{employee.FullName}, Dept:{employee.DepartmentID}, Status:{employee.Status}";

            employee.FirstName = vm.FirstName;
            employee.MiddleName = vm.MiddleName;
            employee.LastName = vm.LastName;
            employee.Suffix = vm.Suffix;
            employee.DateOfBirth = vm.DateOfBirth;
            employee.Gender = vm.Gender;
            employee.CivilStatus = vm.CivilStatus;
            employee.Nationality = vm.Nationality;
            employee.Address = vm.Address;
            employee.ContactNumber = vm.ContactNumber;
            employee.PersonalEmail = vm.PersonalEmail;
            employee.CompanyEmail = vm.CompanyEmail;
            employee.SSSNumber = vm.SSSNumber;
            employee.PhilHealthNumber = vm.PhilHealthNumber;
            employee.PagIbigNumber = vm.PagIbigNumber;
            employee.TINNumber = vm.TINNumber;
            employee.DepartmentID = vm.DepartmentID;
            employee.PositionID = vm.PositionID;
            employee.DateHired = vm.DateHired;
            employee.DateRegularized = vm.DateRegularized;
            employee.EmploymentStatus = vm.EmploymentStatus;
            employee.Status = vm.Status;
            employee.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Update", "Employee",
                $"Updated employee {employee.FullName} ({employee.EmployeeNo})",
                GetClientIP(), oldValues);

            TempData["Success"] = $"Employee {employee.FullName} updated successfully.";
            return RedirectToAction(nameof(Profile), new { id = employee.EmployeeID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> Deactivate(int id, string reason)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            employee.Status = "Inactive";
            employee.DateSeparated = DateTime.Today;
            employee.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(GetCurrentUserID(), GetCurrentUsername(),
                "Deactivate", "Employee",
                $"Deactivated employee {employee.FullName}. Reason: {reason}",
                GetClientIP());

            TempData["Success"] = $"{employee.FullName} has been deactivated.";
            return RedirectToAction(nameof(Index));
        }

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

        private async Task<List<SelectListItem>> GetDepartmentList()
        {
            var depts = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
            return depts.Select(d => new SelectListItem
            {
                Value = d.DepartmentID.ToString(),
                Text = d.DepartmentName
            }).ToList();
        }

        private async Task<List<SelectListItem>> GetPositionList(int departmentId)
        {
            var positions = await _context.Positions
                .Where(p => p.DepartmentID == departmentId && p.IsActive)
                .OrderBy(p => p.PositionTitle)
                .ToListAsync();
            return positions.Select(p => new SelectListItem
            {
                Value = p.PositionID.ToString(),
                Text = p.PositionTitle
            }).ToList();
        }
    }
}
