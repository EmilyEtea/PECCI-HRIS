using PECCI_HRIS.Data;
using PECCI_HRIS.Models;
using Microsoft.EntityFrameworkCore;

namespace PECCI_HRIS.Services
{
    /// <summary>
    /// Background job that runs once after startup.
    /// Only refreshes annual leave credits when it's a new year
    /// (i.e. records for the current year don't exist yet).
    /// This avoids the slow blocking call in Program.cs on every restart.
    /// </summary>
    public class LeaveCreditRefreshJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LeaveCreditRefreshJob> _logger;

        public LeaveCreditRefreshJob(IServiceScopeFactory scopeFactory,
                                     ILogger<LeaveCreditRefreshJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Small delay so the app finishes starting up before we hit the DB
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var leaveSvc = scope.ServiceProvider.GetRequiredService<LeaveCreditService>();

                if (!await db.Database.CanConnectAsync(stoppingToken))
                {
                    _logger.LogWarning("LeaveCreditRefreshJob: Cannot connect to database. Skipping.");
                    return;
                }

                int currentYear = DateTime.Today.Year;

                // Check if any credits already exist for this year
                bool alreadySeeded = await db.LeaveCredits
                    .AnyAsync(lc => lc.Year == currentYear, stoppingToken);

                if (alreadySeeded)
                {
                    _logger.LogInformation(
                        "LeaveCreditRefreshJob: Credits for {Year} already exist. Skipping refresh.", currentYear);
                    return;
                }

                _logger.LogInformation(
                    "LeaveCreditRefreshJob: No credits found for {Year}. Running annual refresh...", currentYear);

                int count = await leaveSvc.RefreshAnnualCredits();

                _logger.LogInformation(
                    "LeaveCreditRefreshJob: Allocated {Count} leave credit record(s) for {Year}.", count, currentYear);
            }
            catch (OperationCanceledException)
            {
                // App is shutting down — normal, ignore
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LeaveCreditRefreshJob: Unexpected error during leave credit refresh.");
            }
        }
    }
}
