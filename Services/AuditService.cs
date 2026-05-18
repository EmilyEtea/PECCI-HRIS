using PECCI_HRIS.Data;
using PECCI_HRIS.Models;

namespace PECCI_HRIS.Services
{
    /// <summary>
    /// Centralized audit logging service.
    /// Every user action (login, create, update, delete) is recorded via this service.
    /// Logs are stored in the AuditLogs table and viewable from Admin → Audit Trail.
    /// </summary>
    public class AuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Writes a single audit log entry asynchronously.
        /// </summary>
        /// <param name="userId">ID of the user performing the action (null for anonymous/system).</param>
        /// <param name="username">Username string for display in the audit trail.</param>
        /// <param name="action">Short action label, e.g. "Create", "Update", "Login".</param>
        /// <param name="module">Module name, e.g. "Employee", "Payroll", "Leave".</param>
        /// <param name="description">Human-readable description of what happened.</param>
        /// <param name="ipAddress">Client IP address (optional).</param>
        /// <param name="oldValues">Serialized old values before the change (optional).</param>
        /// <param name="newValues">Serialized new values after the change (optional).</param>
        public async Task LogAsync(int? userId, string username, string action,
            string module, string description, string? ipAddress = null,
            string? oldValues = null, string? newValues = null)
        {
            var log = new AuditLog
            {
                UserID      = userId,
                Username    = username,
                Action      = action,
                Module      = module,
                Description = description,
                IPAddress   = ipAddress,
                OldValues   = oldValues,
                NewValues   = newValues,
                Timestamp   = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
