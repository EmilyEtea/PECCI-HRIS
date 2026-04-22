using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PECCI_HRIS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditID);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentID);
                });

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    LeaveTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaveTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LeaveCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DefaultDaysPerYear = table.Column<int>(type: "int", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.LeaveTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    SettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SettingGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AllowedValues = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.SettingID);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    PositionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PositionCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionID);
                    table.ForeignKey(
                        name: "FK_Positions_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    PermissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false),
                    CanCreate = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.PermissionID);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CivilStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PersonalEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SSSNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhilHealthNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PagIbigNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TINNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    PositionID = table.Column<int>(type: "int", nullable: false),
                    DateHired = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateRegularized = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateSeparated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmploymentStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProfilePicturePath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Employees_Positions_PositionID",
                        column: x => x.PositionID,
                        principalTable: "Positions",
                        principalColumn: "PositionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    AttendanceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeIn = table.Column<TimeSpan>(type: "time", nullable: true),
                    TimeOut = table.Column<TimeSpan>(type: "time", nullable: true),
                    BreakOut = table.Column<TimeSpan>(type: "time", nullable: true),
                    BreakIn = table.Column<TimeSpan>(type: "time", nullable: true),
                    TotalHoursWorked = table.Column<double>(type: "float", nullable: true),
                    LateMinutes = table.Column<double>(type: "float", nullable: true),
                    OvertimeMinutes = table.Column<double>(type: "float", nullable: true),
                    UndertimeMinutes = table.Column<double>(type: "float", nullable: true),
                    AttendanceStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsManualEntry = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AdjustedBy = table.Column<int>(type: "int", nullable: true),
                    AdjustedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.AttendanceID);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaveApplications",
                columns: table => new
                {
                    LeaveApplicationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    LeaveTypeID = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ManagerApproverID = table.Column<int>(type: "int", nullable: true),
                    ManagerApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerRemarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    HRApproverID = table.Column<int>(type: "int", nullable: true),
                    HRApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HRRemarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveApplications", x => x.LeaveApplicationID);
                    table.ForeignKey(
                        name: "FK_LeaveApplications_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaveApplications_LeaveTypes_LeaveTypeID",
                        column: x => x.LeaveTypeID,
                        principalTable: "LeaveTypes",
                        principalColumn: "LeaveTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaveCredits",
                columns: table => new
                {
                    LeaveCreditID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    LeaveTypeID = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    TotalCredits = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    UsedCredits = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    PendingCredits = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveCredits", x => x.LeaveCreditID);
                    table.ForeignKey(
                        name: "FK_LeaveCredits_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaveCredits_LeaveTypes_LeaveTypeID",
                        column: x => x.LeaveTypeID,
                        principalTable: "LeaveTypes",
                        principalColumn: "LeaveTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRecords",
                columns: table => new
                {
                    PayrollID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    PayPeriod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OvertimePay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HolidayPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NightDifferential = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Allowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SSSContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhilHealthContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PagIbigContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WithholdingTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LeaveDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LateDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UndertimeDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoredGrossPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoredTotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoredNetPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WorkingDays = table.Column<int>(type: "int", nullable: false),
                    DaysWorked = table.Column<int>(type: "int", nullable: false),
                    DaysAbsent = table.Column<int>(type: "int", nullable: false),
                    TotalOvertimeHours = table.Column<double>(type: "float", nullable: false),
                    TotalLateMinutes = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRecords", x => x.PayrollID);
                    table.ForeignKey(
                        name: "FK_PayrollRecords_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "DepartmentID", "CreatedAt", "DepartmentCode", "DepartmentName", "Description", "IsActive" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR", "Human Resources", null, true },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FIN", "Finance & Accounting", null, true },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "IT", "Information Technology", null, true },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OPS", "Operations", null, true },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MKT", "Marketing", null, true }
                });

            migrationBuilder.InsertData(
                table: "LeaveTypes",
                columns: new[] { "LeaveTypeID", "CreatedAt", "DefaultDaysPerYear", "Description", "IsActive", "IsPaid", "LeaveCode", "LeaveTypeName", "RequiresApproval" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15, null, true, true, "VL", "Vacation Leave", true },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15, null, true, true, "SL", "Sick Leave", true },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, null, true, true, "EL", "Emergency Leave", true },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 105, null, true, true, "ML", "Maternity Leave", true },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, null, true, true, "PL", "Paternity Leave", true }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleID", "Description", "IsActive", "RoleName" },
                values: new object[,]
                {
                    { 1, "Full system access", true, "HR Admin" },
                    { 2, "HR operations access", true, "HR Staff" },
                    { 3, "Department management and approvals", true, "Manager" },
                    { 4, "Self-service access", true, "Employee" }
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "SettingID", "AllowedValues", "DataType", "Description", "IsEditable", "SettingGroup", "SettingKey", "SettingValue", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, null, "time", "Official work start time", true, "Attendance", "WorkStartTime", "08:00", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 2, null, "time", "Official work end time", true, "Attendance", "WorkEndTime", "17:00", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 3, null, "int", "Grace period in minutes (08:00 + 5 = 08:05 cutoff)", true, "Attendance", "GracePeriodMinutes", "5", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 4, null, "int", "Additional grace period in seconds", true, "Attendance", "GracePeriodSeconds", "0", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 5, null, "int", "Minimum overtime minutes before OT pay is credited", true, "Attendance", "OvertimeThresholdMinutes", "30", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 6, null, "time", "Lunch break start time", true, "Attendance", "LunchBreakStartTime", "12:00", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 7, null, "time", "Lunch break end time", true, "Attendance", "LunchBreakEndTime", "13:00", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 8, null, "int", "Lunch break duration in minutes", true, "Attendance", "LunchBreakDurationMinutes", "60", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 9, "PerMinute,PerHour,FixedAmount,None", "string", "How late deductions are computed", true, "Attendance", "LateDeductionType", "PerMinute", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 10, null, "decimal", "Fixed deduction per late minute (0 = auto from daily rate)", true, "Attendance", "LateDeductionAmountPerMinute", "0", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 11, "PerMinute,PerHour,FixedAmount,None", "string", "How undertime deductions are computed", true, "Attendance", "UndertimeDeductionType", "PerMinute", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 12, null, "decimal", "Fixed deduction per undertime minute (0 = auto)", true, "Attendance", "UndertimeDeductionAmountPerMinute", "0", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 13, null, "int", "Minutes before work start that time-in is allowed", true, "Attendance", "AllowTimeInBeforeMinutes", "30", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 20, null, "int", "First payroll cutoff day of the month", true, "Payroll", "CutoffDay1", "15", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 21, null, "int", "Second payroll cutoff day of the month", true, "Payroll", "CutoffDay2", "30", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 22, null, "decimal", "Regular overtime rate multiplier (DOLE: 1.25)", true, "Payroll", "OvertimeRateMultiplier", "1.25", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 23, null, "decimal", "Rest day overtime rate multiplier (DOLE: 1.30)", true, "Payroll", "RestDayOvertimeRateMultiplier", "1.30", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 24, null, "decimal", "Special non-working holiday rate multiplier", true, "Payroll", "SpecialHolidayRateMultiplier", "1.30", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 25, null, "decimal", "Regular holiday rate multiplier (DOLE: 2.00)", true, "Payroll", "RegularHolidayRateMultiplier", "2.00", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 26, null, "decimal", "Night differential rate (DOLE: 10% of hourly rate)", true, "Payroll", "NightDifferentialRate", "0.10", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 27, null, "time", "Night differential period start", true, "Payroll", "NightDifferentialStartTime", "22:00", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 28, null, "time", "Night differential period end", true, "Payroll", "NightDifferentialEndTime", "06:00", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 30, null, "decimal", "SSS employee contribution rate (4.5%)", true, "Tax", "SSSEmployeeRate", "0.045", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 31, null, "decimal", "PhilHealth employee share rate (2.5% of basic salary)", true, "Tax", "PhilHealthRate", "0.025", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 32, null, "decimal", "Pag-IBIG employee contribution rate (2%)", true, "Tax", "PagIbigRate", "0.02", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 33, null, "decimal", "Pag-IBIG maximum monthly employee contribution (₱100)", true, "Tax", "PagIbigMaxContribution", "100.00", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 34, "BIR_TRAIN_LAW_2023,BIR_TRAIN_LAW_2018", "string", "BIR withholding tax table to use", true, "Tax", "TaxTableType", "BIR_TRAIN_LAW_2023", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "EmployeeID", "IsActive", "LastLogin", "PasswordHash", "RoleID", "Username" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@pecci.com.ph", null, true, null, "$2a$11$tsh8VDq9WXqELUcBGotmIu5k.tvAIy0MJ6uZcsb9pfAHEWwT.5UvS", 1, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_EmployeeID",
                table: "AttendanceRecords",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentID",
                table: "Employees",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeNo",
                table: "Employees",
                column: "EmployeeNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PositionID",
                table: "Employees",
                column: "PositionID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_EmployeeID",
                table: "LeaveApplications",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_LeaveTypeID",
                table: "LeaveApplications",
                column: "LeaveTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveCredits_EmployeeID_LeaveTypeID_Year",
                table: "LeaveCredits",
                columns: new[] { "EmployeeID", "LeaveTypeID", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveCredits_LeaveTypeID",
                table: "LeaveCredits",
                column: "LeaveTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_EmployeeID",
                table: "PayrollRecords",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_DepartmentID",
                table: "Positions",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleID",
                table: "RolePermissions",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeID",
                table: "Users",
                column: "EmployeeID",
                unique: true,
                filter: "[EmployeeID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                table: "Users",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "LeaveApplications");

            migrationBuilder.DropTable(
                name: "LeaveCredits");

            migrationBuilder.DropTable(
                name: "PayrollRecords");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
