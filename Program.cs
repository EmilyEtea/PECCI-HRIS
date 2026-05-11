using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Configuration;
using PECCI_HRIS.Data;
using PECCI_HRIS.Services;
var builder = WebApplication.CreateBuilder(args);

// ── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// ── Strongly-typed settings ───────────────────────────────────────────────────
builder.Services.Configure<AttendanceSettings>(
    builder.Configuration.GetSection(AttendanceSettings.SectionName));
builder.Services.Configure<PayrollSettings>(
    builder.Configuration.GetSection(PayrollSettings.SectionName));
builder.Services.Configure<KioskSettings>(
    builder.Configuration.GetSection(KioskSettings.SectionName));

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<AttendanceComputationService>();
builder.Services.AddScoped<TaxComputationService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<LeaveCreditService>();
builder.Services.AddScoped<PayslipPdfService>();
builder.Services.AddScoped<ExcelExportService>();
builder.Services.AddScoped<RecurringDeductionService>();
builder.Services.AddScoped<HolidayService>();
builder.Services.AddHostedService<LeaveCreditRefreshJob>();

// ── Authentication ────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Account/Login";
        options.LogoutPath       = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly  = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
// Show detailed errors in browser to help diagnose issues
app.UseDeveloperExceptionPage();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ── Seed admin user + annual leave credit refresh on startup ─────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Seed default admin user (password: Admin@123)
    if (db.Database.CanConnect() && !db.Users.Any(u => u.Username == "admin"))
    {
        db.Users.Add(new PECCI_HRIS.Models.User
        {
            Username     = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Email        = "admin@pecci.com.ph",
            RoleID       = 1,
            IsActive     = true,
            CreatedAt    = DateTime.Now
        });
        db.SaveChanges();
    }
    // Note: Annual leave credit refresh is now handled by LeaveCreditRefreshJob (background service)
    // It runs 5 seconds after startup and only allocates if the current year has no records yet.
}

app.Run();
