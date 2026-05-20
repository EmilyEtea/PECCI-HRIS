using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Data;

namespace PECCI_HRIS.Controllers
{
    [Authorize(Roles = "HR Admin,HR Staff")]
    public class AuditLogController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AuditLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? module, string? action,
            string? username, DateTime? dateFrom, DateTime? dateTo)
        {
            dateFrom ??= DateTime.Today.AddDays(-30);
            dateTo   ??= DateTime.Today;

            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(module))
                query = query.Where(a => a.Module == module);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            if (!string.IsNullOrEmpty(username))
                query = query.Where(a => a.Username != null && a.Username.Contains(username));

            query = query.Where(a => a.Timestamp.Date >= dateFrom.Value.Date &&
                                     a.Timestamp.Date <= dateTo.Value.Date);

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Take(500)
                .ToListAsync();

            ViewBag.Module   = module;
            ViewBag.Action   = action;
            ViewBag.Username = username;
            ViewBag.DateFrom = dateFrom.Value.ToString("yyyy-MM-dd");
            ViewBag.DateTo   = dateTo.Value.ToString("yyyy-MM-dd");
            ViewBag.Modules  = await _context.AuditLogs.Select(a => a.Module).Distinct().OrderBy(m => m).ToListAsync();

            return View(logs);
        }
    }
}
