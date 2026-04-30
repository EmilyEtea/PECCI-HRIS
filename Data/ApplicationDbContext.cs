using Microsoft.EntityFrameworkCore;
using PECCI_HRIS.Models;

namespace PECCI_HRIS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveCredit> LeaveCredits { get; set; }
        public DbSet<LeaveApplication> LeaveApplications { get; set; }
        public DbSet<PayrollRecord> PayrollRecords { get; set; }
        public DbSet<EmployeeDeduction> EmployeeDeductions { get; set; }
        public DbSet<RecurringDeductionSchedule> RecurringDeductionSchedules { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.EmployeeNo).IsUnique();

            modelBuilder.Entity<LeaveCredit>()
                .HasIndex(lc => new { lc.EmployeeID, lc.LeaveTypeID, lc.Year }).IsUnique();

            // Fix cascade delete cycles (SQL Server error 1785)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Position>()
                .HasOne(p => p.Department)
                .WithMany(d => d.Positions)
                .HasForeignKey(p => p.DepartmentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.AttendanceRecords)
                .HasForeignKey(a => a.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveApplication>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.LeaveApplications)
                .HasForeignKey(l => l.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveApplication>()
                .HasOne(l => l.LeaveType)
                .WithMany(lt => lt.LeaveApplications)
                .HasForeignKey(l => l.LeaveTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveCredit>()
                .HasOne(lc => lc.Employee)
                .WithMany(e => e.LeaveCredits)
                .HasForeignKey(lc => lc.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveCredit>()
                .HasOne(lc => lc.LeaveType)
                .WithMany(lt => lt.LeaveCredits)
                .HasForeignKey(lc => lc.LeaveTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PayrollRecord>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.PayrollRecords)
                .HasForeignKey(p => p.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithOne(e => e.UserAccount)
                .HasForeignKey<User>(u => u.EmployeeID)
                .OnDelete(DeleteBehavior.SetNull);

            // Decimal precision
            modelBuilder.Entity<Position>()
                .Property(p => p.BasicSalary).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.BasicSalary).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.OvertimePay).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.HolidayPay).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.NightDifferential).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.Allowances).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.OtherEarnings).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.SSSContribution).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.PhilHealthContribution).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.PagIbigContribution).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.WithholdingTax).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.LeaveDeductions).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.LateDeductions).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.UndertimeDeductions).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.OtherDeductions).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.StoredGrossPay).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.StoredTotalDeductions).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PayrollRecord>()
                .Property(p => p.StoredNetPay).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<LeaveCredit>()
                .Property(lc => lc.TotalCredits).HasColumnType("decimal(5,1)");
            modelBuilder.Entity<LeaveCredit>()
                .Property(lc => lc.UsedCredits).HasColumnType("decimal(5,1)");
            modelBuilder.Entity<LeaveCredit>()
                .Property(lc => lc.PendingCredits).HasColumnType("decimal(5,1)");

            modelBuilder.Entity<LeaveApplication>()
                .Property(la => la.NumberOfDays).HasColumnType("decimal(5,1)");

            // EmployeeDeduction
            modelBuilder.Entity<EmployeeDeduction>()
                .HasOne(d => d.Employee)
                .WithMany(e => e.EmployeeDeductions)
                .HasForeignKey(d => d.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeDeduction>()
                .Property(d => d.Amount).HasColumnType("decimal(18,2)");

            // RecurringDeductionSchedule
            modelBuilder.Entity<RecurringDeductionSchedule>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RecurringDeductionSchedule>()
                .Property(s => s.AmountPerCutoff).HasColumnType("decimal(18,2)");

            // Seed data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // ── System Settings (adjustable from admin UI) ───────────────────────
            modelBuilder.Entity<SystemSetting>().HasData(
                // Attendance
                new SystemSetting { SettingID = 1,  SettingKey = "WorkStartTime",                  SettingValue = "08:00",        SettingGroup = "Attendance", DataType = "time",    Description = "Official work start time",                                    IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 2,  SettingKey = "WorkEndTime",                    SettingValue = "17:00",        SettingGroup = "Attendance", DataType = "time",    Description = "Official work end time",                                      IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 3,  SettingKey = "GracePeriodMinutes",             SettingValue = "5",            SettingGroup = "Attendance", DataType = "int",     Description = "Grace period in minutes (08:00 + 5 = 08:05 cutoff)",          IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 4,  SettingKey = "GracePeriodSeconds",             SettingValue = "0",            SettingGroup = "Attendance", DataType = "int",     Description = "Additional grace period in seconds",                          IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 5,  SettingKey = "OvertimeThresholdMinutes",       SettingValue = "30",           SettingGroup = "Attendance", DataType = "int",     Description = "Minimum overtime minutes before OT pay is credited",          IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 6,  SettingKey = "LunchBreakStartTime",            SettingValue = "12:00",        SettingGroup = "Attendance", DataType = "time",    Description = "Lunch break start time",                                      IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 7,  SettingKey = "LunchBreakEndTime",              SettingValue = "13:00",        SettingGroup = "Attendance", DataType = "time",    Description = "Lunch break end time",                                        IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 8,  SettingKey = "LunchBreakDurationMinutes",      SettingValue = "60",           SettingGroup = "Attendance", DataType = "int",     Description = "Lunch break duration in minutes",                             IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 9,  SettingKey = "LateDeductionType",              SettingValue = "PerMinute",    SettingGroup = "Attendance", DataType = "string",  Description = "How late deductions are computed",                            IsEditable = true, AllowedValues = "PerMinute,PerHour,FixedAmount,None", UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 10, SettingKey = "LateDeductionAmountPerMinute",   SettingValue = "0",            SettingGroup = "Attendance", DataType = "decimal", Description = "Fixed deduction per late minute (0 = auto from daily rate)",  IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 11, SettingKey = "UndertimeDeductionType",         SettingValue = "PerMinute",    SettingGroup = "Attendance", DataType = "string",  Description = "How undertime deductions are computed",                       IsEditable = true, AllowedValues = "PerMinute,PerHour,FixedAmount,None", UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 12, SettingKey = "UndertimeDeductionAmountPerMinute", SettingValue = "0",         SettingGroup = "Attendance", DataType = "decimal", Description = "Fixed deduction per undertime minute (0 = auto)",            IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 13, SettingKey = "AllowTimeInBeforeMinutes",       SettingValue = "30",           SettingGroup = "Attendance", DataType = "int",     Description = "Minutes before work start that time-in is allowed",           IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },

                // Payroll
                new SystemSetting { SettingID = 20, SettingKey = "CutoffDay1",                     SettingValue = "15",           SettingGroup = "Payroll",    DataType = "int",     Description = "First payroll cutoff day of the month",                       IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 21, SettingKey = "CutoffDay2",                     SettingValue = "30",           SettingGroup = "Payroll",    DataType = "int",     Description = "Second payroll cutoff day of the month",                      IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 22, SettingKey = "OvertimeRateMultiplier",         SettingValue = "1.25",         SettingGroup = "Payroll",    DataType = "decimal", Description = "Regular overtime rate multiplier (DOLE: 1.25)",               IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 23, SettingKey = "RestDayOvertimeRateMultiplier",  SettingValue = "1.30",         SettingGroup = "Payroll",    DataType = "decimal", Description = "Rest day overtime rate multiplier (DOLE: 1.30)",              IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 24, SettingKey = "SpecialHolidayRateMultiplier",   SettingValue = "1.30",         SettingGroup = "Payroll",    DataType = "decimal", Description = "Special non-working holiday rate multiplier",                 IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 25, SettingKey = "RegularHolidayRateMultiplier",   SettingValue = "2.00",         SettingGroup = "Payroll",    DataType = "decimal", Description = "Regular holiday rate multiplier (DOLE: 2.00)",                IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 26, SettingKey = "NightDifferentialRate",          SettingValue = "0.10",         SettingGroup = "Payroll",    DataType = "decimal", Description = "Night differential rate (DOLE: 10% of hourly rate)",          IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 27, SettingKey = "NightDifferentialStartTime",     SettingValue = "22:00",        SettingGroup = "Payroll",    DataType = "time",    Description = "Night differential period start",                             IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 28, SettingKey = "NightDifferentialEndTime",       SettingValue = "06:00",        SettingGroup = "Payroll",    DataType = "time",    Description = "Night differential period end",                               IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },

                // Tax / Government Contributions
                new SystemSetting { SettingID = 30, SettingKey = "SSSEmployeeRate",                SettingValue = "0.045",        SettingGroup = "Tax",        DataType = "decimal", Description = "SSS employee contribution rate (4.5%)",                       IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 31, SettingKey = "PhilHealthRate",                 SettingValue = "0.025",        SettingGroup = "Tax",        DataType = "decimal", Description = "PhilHealth employee share rate (2.5% of basic salary)",       IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 32, SettingKey = "PagIbigRate",                    SettingValue = "0.02",         SettingGroup = "Tax",        DataType = "decimal", Description = "Pag-IBIG employee contribution rate (2%)",                    IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 33, SettingKey = "PagIbigMaxContribution",         SettingValue = "100.00",       SettingGroup = "Tax",        DataType = "decimal", Description = "Pag-IBIG maximum monthly employee contribution (₱100)",       IsEditable = true, UpdatedAt = new DateTime(2026,1,1) },
                new SystemSetting { SettingID = 34, SettingKey = "TaxTableType",                   SettingValue = "BIR_TRAIN_LAW_2023", SettingGroup = "Tax", DataType = "string",  Description = "BIR withholding tax table to use",                            IsEditable = true, AllowedValues = "BIR_TRAIN_LAW_2023,BIR_TRAIN_LAW_2018", UpdatedAt = new DateTime(2026,1,1) }
            );

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleID = 1, RoleName = "HR Admin", Description = "Full system access", IsActive = true },
                new Role { RoleID = 2, RoleName = "HR Staff", Description = "HR operations access", IsActive = true },
                new Role { RoleID = 3, RoleName = "Manager", Description = "Department management and approvals", IsActive = true },
                new Role { RoleID = 4, RoleName = "Employee", Description = "Self-service access", IsActive = true }
            );

            // NOTE: Default admin user is seeded in Program.cs after migration
            // to avoid non-deterministic BCrypt hash in HasData()

            // Seed Departments
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentID = 1, DepartmentName = "Human Resources", DepartmentCode = "HR", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new Department { DepartmentID = 2, DepartmentName = "Finance & Accounting", DepartmentCode = "FIN", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new Department { DepartmentID = 3, DepartmentName = "Information Technology", DepartmentCode = "IT", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new Department { DepartmentID = 4, DepartmentName = "Operations", DepartmentCode = "OPS", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new Department { DepartmentID = 5, DepartmentName = "Marketing", DepartmentCode = "MKT", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) }
            );

            // Seed Leave Types
            modelBuilder.Entity<LeaveType>().HasData(
                new LeaveType { LeaveTypeID = 1, LeaveTypeName = "Vacation Leave", LeaveCode = "VL", DefaultDaysPerYear = 15, IsPaid = true, RequiresApproval = true, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new LeaveType { LeaveTypeID = 2, LeaveTypeName = "Sick Leave", LeaveCode = "SL", DefaultDaysPerYear = 15, IsPaid = true, RequiresApproval = true, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new LeaveType { LeaveTypeID = 3, LeaveTypeName = "Emergency Leave", LeaveCode = "EL", DefaultDaysPerYear = 3, IsPaid = true, RequiresApproval = true, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new LeaveType { LeaveTypeID = 4, LeaveTypeName = "Maternity Leave", LeaveCode = "ML", DefaultDaysPerYear = 105, IsPaid = true, RequiresApproval = true, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new LeaveType { LeaveTypeID = 5, LeaveTypeName = "Paternity Leave", LeaveCode = "PL", DefaultDaysPerYear = 7, IsPaid = true, RequiresApproval = true, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) }
            );
        }
    }
}
