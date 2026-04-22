using PECCI_HRIS.Data;
using PECCI_HRIS.Models;

namespace PECCI_HRIS.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

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
