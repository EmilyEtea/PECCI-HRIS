# PECCI HRIS — Technical Documentation
## PECCI Multipurpose Cooperative
### Human Resource Information System

---

**Version:** 1.0.0
**Year:** 2026
**Developed by:** UST Interns — Arkin Reinier Aguilar, Maxenne De Guzman, Bernice Elyssa Soriano, Emily Etea

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Technology Stack](#2-technology-stack)
3. [System Architecture](#3-system-architecture)
4. [Database Design](#4-database-design)
5. [Module Documentation](#5-module-documentation)
6. [Business Rules & Computations](#6-business-rules--computations)
7. [Security Implementation](#7-security-implementation)
8. [Configuration & Settings](#8-configuration--settings)
9. [Installation Guide](#9-installation-guide)
10. [User Roles & Permissions](#10-user-roles--permissions)
11. [API Reference](#11-api-reference)
12. [Known Issues & Limitations](#12-known-issues--limitations)

---

## 1. System Overview

The PECCI HRIS (Human Resource Information System) is a web-based application developed for PECCI Multipurpose Cooperative (PECCI) to centralize and streamline HR operations. The system covers the full employee lifecycle from onboarding to payroll processing.

### 1.1 Purpose
- Centralize employee records management
- Automate attendance tracking and computation
- Streamline leave application and approval workflows
- Automate payroll computation with Philippine regulatory compliance
- Provide management reports and audit trails

### 1.2 Scope
The system covers:
- Employee information management (CRUD)
- Department and position management
- Attendance tracking (Time In/Out with grace period logic)
- Leave management with approval workflow
- Payroll computation (BIR TRAIN Law compliant)
- Reports generation
- User account management with role-based access
- System settings management
- Audit trail logging

---

## 2. Technology Stack

| Component | Technology | Version |
|---|---|---|
| Framework | ASP.NET Core MVC | .NET 10.0 |
| ORM | Entity Framework Core | 9.0.4 |
| Database | SQL Server / LocalDB | 2019+ |
| Authentication | Cookie-based (ASP.NET Core) | Built-in |
| Password Hashing | BCrypt.Net-Next | 4.0.3 |
| PDF Generation | iText7 | 8.0.4 |
| Excel Export | ClosedXML | 0.102.2 |
| Frontend Framework | Bootstrap | 5.3.3 |
| Icons | Font Awesome | 6.5.0 |
| Charts | Chart.js | 4.4.2 |
| IDE | Visual Studio Code / Visual Studio 2022 | — |

---

## 3. System Architecture

### 3.1 MVC Pattern
The system follows the Model-View-Controller (MVC) architectural pattern:

```
PECCI_HRIS/
├── Configuration/          # Strongly-typed settings (AttendanceSettings, PayrollSettings)
├── Controllers/            # 13 MVC controllers
│   ├── AccountController   # Authentication (Login, Logout, Profile)
│   ├── DashboardController # Dashboard statistics
│   ├── EmployeeController  # Employee CRUD + Profile
│   ├── DepartmentController# Department management
│   ├── PositionController  # Position management
│   ├── AttendanceController# Time In/Out, Adjust, Summary
│   ├── LeaveController     # Leave applications, credits, types
│   ├── PayrollController   # Payroll computation, payslips
│   ├── ReportsController   # Report generation
│   ├── UsersController     # User account management
│   ├── SettingsController  # System settings
│   ├── AuditLogController  # Audit trail viewer
│   └── BaseController      # Shared helper methods
├── Data/
│   └── ApplicationDbContext# EF Core DbContext with seed data
├── Models/                 # 13 entity models
├── Services/               # 3 business logic services
│   ├── AttendanceComputationService
│   ├── TaxComputationService
│   └── AuditService
├── ViewModels/             # 11 view model classes
├── Views/                  # 50+ Razor views
└── wwwroot/css/            # PECCI brand CSS theme
```

### 3.2 Dependency Injection
All services are registered in `Program.cs`:
```csharp
builder.Services.AddScoped<AttendanceComputationService>();
builder.Services.AddScoped<TaxComputationService>();
builder.Services.AddScoped<AuditService>();
```

### 3.3 Configuration Binding
Settings are bound to strongly-typed classes:
```csharp
builder.Services.Configure<AttendanceSettings>(
    builder.Configuration.GetSection("AttendanceSettings"));
builder.Services.Configure<PayrollSettings>(
    builder.Configuration.GetSection("PayrollSettings"));
```

---

## 4. Database Design

### 4.1 Entity Relationship Overview

```
Users ──────────── Roles
  │
  └── Employees ──── Departments
         │               │
         │           Positions
         │
         ├── AttendanceRecords
         ├── LeaveApplications ──── LeaveTypes
         ├── LeaveCredits ────────── LeaveTypes
         └── PayrollRecords

SystemSettings (standalone — stores adjustable rules)
AuditLogs (standalone — stores all user actions)
RolePermissions ──── Roles
```

### 4.2 Table Descriptions

#### Users
| Column | Type | Description |
|---|---|---|
| UserID | int PK | Auto-increment primary key |
| Username | nvarchar(50) UNIQUE | Login username |
| PasswordHash | nvarchar(max) | BCrypt hashed password |
| Email | nvarchar(100) UNIQUE | User email |
| RoleID | int FK | Reference to Roles |
| EmployeeID | int FK nullable | Linked employee record |
| IsActive | bit | Account status |
| LastLogin | datetime nullable | Last successful login |

#### Employees
| Column | Type | Description |
|---|---|---|
| EmployeeID | int PK | Auto-increment |
| EmployeeNo | nvarchar(20) UNIQUE | Employee number (e.g., EMP-0001) |
| FirstName | nvarchar(50) | First name |
| MiddleName | nvarchar(50) nullable | Middle name |
| LastName | nvarchar(50) | Last name |
| DateOfBirth | datetime | Date of birth |
| Gender | nvarchar(10) | Male/Female |
| DepartmentID | int FK | Department assignment |
| PositionID | int FK | Position assignment |
| DateHired | datetime | Employment start date |
| EmploymentStatus | nvarchar(30) | Regular/Probationary/Contractual/Part-time |
| Status | nvarchar(20) | Active/Inactive/Resigned/Terminated/Retired |
| SSSNumber | nvarchar(20) nullable | SSS ID |
| PhilHealthNumber | nvarchar(20) nullable | PhilHealth ID |
| PagIbigNumber | nvarchar(20) nullable | Pag-IBIG ID |
| TINNumber | nvarchar(20) nullable | BIR TIN |

#### AttendanceRecords
| Column | Type | Description |
|---|---|---|
| AttendanceID | int PK | Auto-increment |
| EmployeeID | int FK | Employee reference |
| AttendanceDate | datetime | Date of attendance |
| TimeIn | time nullable | Time-in timestamp |
| TimeOut | time nullable | Time-out timestamp |
| LateMinutes | float nullable | Computed late minutes |
| OvertimeMinutes | float nullable | Computed overtime minutes |
| UndertimeMinutes | float nullable | Computed undertime minutes |
| TotalHoursWorked | float nullable | Computed total hours |
| AttendanceStatus | nvarchar(20) | Present/Late/Absent/On Leave/Holiday |
| IsManualEntry | bit | Whether manually adjusted |
| AdjustedBy | int nullable | UserID who adjusted |

#### PayrollRecords
| Column | Type | Description |
|---|---|---|
| PayrollID | int PK | Auto-increment |
| EmployeeID | int FK | Employee reference |
| PayPeriod | nvarchar(20) | e.g., "2026-01-1-15" |
| PeriodStart | datetime | Cutoff period start |
| PeriodEnd | datetime | Cutoff period end |
| BasicSalary | decimal(18,2) | Semi-monthly basic |
| OvertimePay | decimal(18,2) | Computed OT pay |
| SSSContribution | decimal(18,2) | SSS employee share |
| PhilHealthContribution | decimal(18,2) | PhilHealth employee share |
| PagIbigContribution | decimal(18,2) | Pag-IBIG employee share |
| WithholdingTax | decimal(18,2) | BIR withholding tax |
| LateDeductions | decimal(18,2) | Late deduction amount |
| StoredGrossPay | decimal(18,2) | Computed gross pay |
| StoredTotalDeductions | decimal(18,2) | Computed total deductions |
| StoredNetPay | decimal(18,2) | Computed net pay |
| Status | nvarchar(20) | Draft/Finalized/Released |

---

## 5. Module Documentation

### 5.1 Authentication Module (Sprint 1)

**Controller:** `AccountController`

| Action | Method | Description |
|---|---|---|
| Login (GET) | GET | Display login form |
| Login (POST) | POST | Authenticate user, create cookie session |
| Logout | POST | Sign out, clear session |
| Profile | GET | View current user profile |
| AccessDenied | GET | Access denied page |

**Authentication Flow:**
1. User submits username + password
2. System looks up user by username (active only)
3. BCrypt.Verify() checks password against stored hash
4. On success: creates ClaimsPrincipal with role, name, employeeID claims
5. Cookie issued with 8-hour expiry (30 days if "Remember Me")
6. Failed attempts are logged to AuditLog

### 5.2 Employee Management Module (Sprint 1)

**Controller:** `EmployeeController`

| Action | Method | Access | Description |
|---|---|---|---|
| Index | GET | All | List employees with search/filter |
| Profile | GET | All | View employee profile |
| Create (GET) | GET | HR Admin, HR Staff | Add employee form |
| Create (POST) | POST | HR Admin, HR Staff | Save new employee |
| Edit (GET) | GET | HR Admin, HR Staff | Edit employee form |
| Edit (POST) | POST | HR Admin, HR Staff | Update employee |
| Deactivate | POST | HR Admin | Deactivate employee |
| GetPositionsByDepartment | GET | All | AJAX — load positions by dept |

**Auto-features on Create:**
- Employee number auto-generated if blank (EMP-XXXX format)
- Leave credits automatically allocated for current year (all active leave types)

### 5.3 Attendance Module (Sprint 2)

**Controller:** `AttendanceController`
**Service:** `AttendanceComputationService`

| Action | Method | Access | Description |
|---|---|---|---|
| Index | GET | All | Attendance list with filters |
| TimeIn | POST | All | Record time-in |
| TimeOut | POST | All | Record time-out |
| Adjust | GET | HR Admin, HR Staff | Manual adjustment form |
| Adjust (POST) | POST | HR Admin, HR Staff | Save adjustment + recompute |
| Summary | GET | HR Admin, HR Staff | Monthly summary report |

**Computation Logic (AttendanceComputationService):**
- Late threshold = WorkStart + GracePeriodMinutes + GracePeriodSeconds
- Late minutes = ceil(TimeIn - WorkStart) if TimeIn > LateThreshold
- Overtime = floor(TimeOut - WorkEnd) if TimeOut > WorkEnd AND minutes ≥ OvertimeThreshold
- Undertime = ceil(WorkEnd - TimeOut) if TimeOut < WorkEnd
- Hours worked = (TimeOut - TimeIn) - LunchBreakDuration (if worked through lunch)

### 5.4 Leave Management Module (Sprint 3)

**Controller:** `LeaveController`

| Action | Method | Access | Description |
|---|---|---|---|
| Index | GET | All | Leave applications list |
| Apply (GET) | GET | All | Leave application form |
| Apply (POST) | POST | All | Submit leave application |
| Review (GET) | GET | HR Admin, HR Staff, Manager | Review application |
| Review (POST) | POST | HR Admin, HR Staff, Manager | Approve/Reject |
| Cancel | POST | All (own) | Cancel pending application |
| Credits | GET | All | View leave credits |
| Types | GET | HR Admin, HR Staff | Leave types list |
| CreateType | GET/POST | HR Admin | Add leave type |

**Approval Workflow:**
1. Employee applies → Status: "Pending"
2. Manager reviews → Status: "Pending HR" (approved) or "Rejected"
3. HR reviews → Status: "Approved" or "Rejected"
4. On Approval: UsedCredits += NumberOfDays, PendingCredits -= NumberOfDays
5. On Rejection: PendingCredits -= NumberOfDays (released)

### 5.5 Payroll Module (Sprint 4)

**Controller:** `PayrollController`
**Service:** `TaxComputationService`, `AttendanceComputationService`

| Action | Method | Access | Description |
|---|---|---|---|
| Index | GET | HR Admin, HR Staff | Payroll records list |
| Compute (GET) | GET | HR Admin, HR Staff | Computation parameters form |
| Compute (POST) | POST | HR Admin, HR Staff | Run payroll computation |
| Payslips | GET | HR Admin, HR Staff | View payslips |
| Finalize | POST | HR Admin | Finalize payroll record |

**Computation Steps:**
1. Determine period dates from cutoff selection
2. Get employee's position basic salary (÷2 for semi-monthly)
3. Retrieve attendance records for the period
4. Compute: OT pay, late deductions, undertime deductions, absent deductions
5. Compute government contributions (SSS, PhilHealth, Pag-IBIG) on monthly basis ÷2
6. Compute withholding tax on monthly taxable income ÷2
7. Store gross pay, total deductions, net pay

---

## 6. Business Rules & Computations

### 6.1 Attendance Rules

#### Grace Period
```
LateThreshold = WorkStartTime + GracePeriodMinutes + GracePeriodSeconds
Default: 08:00 + 5 min + 0 sec = 08:05:00

If TimeIn > LateThreshold → LATE
LateMinutes = ceil(TimeIn - WorkStartTime)  ← counted from 08:00, NOT from 08:05
```

**Example:**
- Time In: 08:06 → Late by 6 minutes (from 08:00)
- Time In: 08:05:00 → On time
- Time In: 08:05:01 → Late by 5 minutes (from 08:00)

#### Overtime
```
If TimeOut > WorkEndTime:
    OvertimeMinutes = floor(TimeOut - WorkEndTime)
    If OvertimeMinutes >= OvertimeThresholdMinutes → credited
    Else → not credited (below threshold)
```

#### Deductions
```
Daily Rate = MonthlySalary / 22 working days
Per-Minute Rate = Daily Rate / WorkingMinutesPerDay

Late Deduction = LateMinutes × Per-Minute Rate
Undertime Deduction = UndertimeMinutes × Per-Minute Rate
Absent Deduction = DaysAbsent × Daily Rate
```

### 6.2 BIR TRAIN Law Tax Computation (RA 10963, RR 2-2023)

**Annual Tax Brackets (effective January 1, 2023):**

| Annual Taxable Income | Tax Formula |
|---|---|
| ≤ ₱250,000 | ₱0 (Exempt) |
| ₱250,001 – ₱400,000 | (Income - 250,000) × 15% |
| ₱400,001 – ₱800,000 | ₱22,500 + (Income - 400,000) × 20% |
| ₱800,001 – ₱2,000,000 | ₱102,500 + (Income - 800,000) × 25% |
| ₱2,000,001 – ₱8,000,000 | ₱402,500 + (Income - 2,000,000) × 30% |
| > ₱8,000,000 | ₱2,202,500 + (Income - 8,000,000) × 35% |

**Monthly Tax = Annual Tax / 12**

**Taxable Income = Gross Income - SSS - PhilHealth - Pag-IBIG**

### 6.3 SSS Contribution (2026)

Employee share = 4.5% of Monthly Salary Credit (MSC)
MSC range: ₱4,000 – ₱30,000 (stepped brackets)

### 6.4 PhilHealth Contribution (2026)

Total premium = Basic Salary × 5%
Employee share = Total Premium / 2 (2.5%)
Minimum: ₱500/month | Maximum: ₱5,000/month

### 6.5 Pag-IBIG Contribution

- Salary ≤ ₱1,500: Employee rate = 1%
- Salary > ₱1,500: Employee rate = 2%
- Maximum employee contribution: ₱100/month

### 6.6 Overtime Pay Rates (DOLE)

| Type | Multiplier |
|---|---|
| Regular overtime | 1.25× hourly rate |
| Rest day overtime | 1.30× hourly rate |
| Special holiday | 1.30× hourly rate |
| Regular holiday | 2.00× hourly rate |
| Night differential | +10% of hourly rate (10pm–6am) |

---

## 7. Security Implementation

### 7.1 Authentication
- Cookie-based authentication (ASP.NET Core Identity-free)
- Session expires after 8 hours (sliding expiration)
- "Remember Me" extends to 30 days
- HttpOnly cookies prevent JavaScript access
- SecurePolicy = SameAsRequest (HTTPS in production)

### 7.2 Password Security
- BCrypt hashing with default cost factor (11 rounds)
- Passwords never stored in plain text
- Minimum 8 characters enforced on creation/reset

### 7.3 Authorization
- Role-based authorization via `[Authorize(Roles = "...")]`
- Four roles: HR Admin, HR Staff, Manager, Employee
- Employees can only view their own records
- HR Admin has full system access

### 7.4 Input Validation
- Server-side validation via Data Annotations
- Anti-forgery tokens on all POST forms (`@Html.AntiForgeryToken()`)
- EF Core parameterized queries prevent SQL injection
- Model binding prevents over-posting

### 7.5 Audit Trail
All user actions are logged to `AuditLogs` table:
- UserID, Username, Action, Module, Description
- IP Address, Timestamp
- Old values and new values (for updates)

---

## 8. Configuration & Settings

### 8.1 Connection String
File: `appsettings.json`
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=PECCI_HRIS_DB;..."
}
```

### 8.2 Adjustable Settings
All settings stored in `SystemSettings` table and editable via Admin → Settings UI.

**Attendance Settings:**
| Key | Default | Description |
|---|---|---|
| WorkStartTime | 08:00 | Official work start |
| WorkEndTime | 17:00 | Official work end |
| GracePeriodMinutes | 5 | Grace period (08:00 + 5 = 08:05 cutoff) |
| GracePeriodSeconds | 0 | Additional seconds |
| OvertimeThresholdMinutes | 30 | Min OT before pay credited |
| LateDeductionType | PerMinute | PerMinute/PerHour/FixedAmount/None |
| LateDeductionAmountPerMinute | 0 | 0 = auto from daily rate |

**Payroll Settings:**
| Key | Default | Description |
|---|---|---|
| CutoffDay1 | 15 | First cutoff day |
| CutoffDay2 | 30 | Second cutoff day |
| OvertimeRateMultiplier | 1.25 | Regular OT rate |
| RegularHolidayRateMultiplier | 2.00 | Holiday rate |
| NightDifferentialRate | 0.10 | Night diff rate |

**Tax Settings:**
| Key | Default | Description |
|---|---|---|
| TaxTableType | BIR_TRAIN_LAW_2023 | Tax table to use |
| SSSEmployeeRate | 0.045 | SSS employee share |
| PhilHealthRate | 0.025 | PhilHealth employee share |
| PagIbigRate | 0.02 | Pag-IBIG rate |
| PagIbigMaxContribution | 100.00 | Max Pag-IBIG contribution |

---

## 9. Installation Guide

### 9.1 Prerequisites
- .NET 10.0 SDK
- SQL Server 2019+ or LocalDB
- Visual Studio Code or Visual Studio 2022
- SQL Server Management Studio (SSMS) — optional

### 9.2 Setup Steps

**Step 1: Clone Repository**
```bash
git clone https://github.com/EmilyEtea/PECCI-HRIS.git
cd PECCI-HRIS/PECCI_HRIS
```

**Step 2: Configure Connection String**
Edit `appsettings.json`:
```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=PECCI_HRIS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
```

**Step 3: Restore Packages**
```bash
dotnet restore
```

**Step 4: Run Migrations**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Step 5: Run Application**
```bash
dotnet run
```

**Step 6: Login**
- URL: `https://localhost:5001`
- Username: `admin`
- Password: `Admin@123`

### 9.3 Common Connection Strings

| Scenario | Connection String |
|---|---|
| LocalDB | `Server=(localdb)\\MSSQLLocalDB;Database=PECCI_HRIS_DB;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Express | `Server=localhost\\SQLEXPRESS;Database=PECCI_HRIS_DB;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Auth | `Server=localhost;Database=PECCI_HRIS_DB;User Id=sa;Password=YourPwd;TrustServerCertificate=True;` |

---

## 10. User Roles & Permissions

| Module | HR Admin | HR Staff | Manager | Employee |
|---|---|---|---|---|
| Dashboard | ✅ Full | ✅ Full | ✅ Full | ✅ Own |
| Employee List | ✅ Full | ✅ Full | ✅ View | ✅ Own |
| Employee CRUD | ✅ Full | ✅ Create/Edit | ❌ | ❌ |
| Employee Deactivate | ✅ | ❌ | ❌ | ❌ |
| Departments | ✅ Full | ✅ Create/Edit | ❌ | ❌ |
| Positions | ✅ Full | ✅ Create/Edit | ❌ | ❌ |
| Attendance View | ✅ All | ✅ All | ✅ Dept | ✅ Own |
| Attendance Adjust | ✅ | ✅ | ❌ | ❌ |
| Leave Apply | ✅ | ✅ | ✅ | ✅ |
| Leave Approve | ✅ | ✅ | ✅ (Manager step) | ❌ |
| Leave Types | ✅ Full | ✅ View | ❌ | ❌ |
| Payroll | ✅ Full | ✅ Compute/View | ❌ | ❌ |
| Payroll Finalize | ✅ | ❌ | ❌ | ❌ |
| Reports | ✅ All | ✅ All | ✅ All | ❌ |
| User Management | ✅ | ❌ | ❌ | ❌ |
| System Settings | ✅ | ❌ | ❌ | ❌ |
| Audit Trail | ✅ | ❌ | ❌ | ❌ |

---

## 11. API Reference

### AJAX Endpoints

#### GET /Employee/GetPositionsByDepartment
Returns positions for a given department (used in employee form dropdowns).

**Parameters:** `departmentId` (int)

**Response:**
```json
[
  { "positionID": 1, "positionTitle": "HR Manager", "basicSalary": 65000.00 }
]
```

#### POST /Settings/UpdateSetting
Updates a single system setting.

**Body:**
```json
{ "settingID": 3, "settingKey": "GracePeriodMinutes", "settingValue": "10" }
```

**Response:**
```json
{ "success": true, "message": "GracePeriodMinutes updated successfully." }
```

#### GET /Settings/PreviewAttendanceRule
Returns computed late threshold for preview.

**Parameters:** `workStart`, `workEnd`, `graceMins`, `graceSecs`, `otThreshold`

**Response:**
```json
{ "success": true, "lateThreshold": "08:10:00", "description": "..." }
```

---

## 12. Known Issues & Limitations

1. **PDF Payslip Generation** — iText7 is included as a dependency but the PDF export action is not yet implemented. Payslips can be printed via browser print.

2. **Excel Export** — ClosedXML is included but export buttons are not yet wired to controllers. Reports can be printed via browser.

3. **Email Notifications** — Leave approval notifications via email are not implemented in this version.

4. **Biometric Integration** — Time In/Out is manual (web-based). Biometric device integration is not in scope for this version.

5. **Holiday Calendar** — Regular and special holidays must be manually marked in attendance records. An automated holiday calendar is planned for a future sprint.

6. **Night Differential** — Night differential computation is defined in settings but not automatically applied in the current payroll computation. Manual entry via OtherEarnings is required.

---

*Document prepared by UST Interns: Arkin Reinier Aguilar, Maxenne De Guzman, Bernice Elyssa Soriano, Emily Etea*
*© 2026 PECCI — PECCI Multipurpose Cooperative*
