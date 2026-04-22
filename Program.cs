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

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<AttendanceComputationService>();
builder.Services.AddScoped<TaxComputationService>();
builder.Services.AddScoped<AuditService>();

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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

// ── Seed admin user on startup ───────────────────────────────────────────────
// Migrations are applied via CLI: dotnet ef database update
// This block only seeds the default admin user if it doesn't exist yet.
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
}

app.Run();
