# PECCI HRIS — Technical Documentation
## PECCI Multipurpose Cooperative
### Human Resource Information System

---

**Version:** 1.1.0
**Year:** 2026
**Developed by:** UST Interns — Arkin Reinier Aguilar, Maxenne De Guzman, Bernice Elyssa Soriano, Emily Etea
**Repository:** https://github.com/EmilyEtea/PECCI-HRIS

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

> **How to export as PDF:**
> Open this file in VS Code, install the **Markdown PDF** extension (yzane.markdown-pdf),
> right-click → *Markdown PDF: Export (pdf)*. All tables, diagrams, and code blocks render cleanly.

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
| Excel Export | ClosedXML | 0.104.2 |
| Frontend Framework | Bootstrap | 5.3.3 |
| Icons | Font Awesome | 6.5.0 |
| Charts | Chart.js | 4.4.2 |
| IDE | Visual Studio Code / Visual Studio 2022 | — |
| Version Control | Git / GitHub | — |

---

## 3. System Architecture

### 3.1 MVC Pattern
The system follows the Model-View-Controller (MVC) architectural pattern:

```
PECCI_HRIS/
├── Configuration/           # Strongly-typed settings
│   ├── AttendanceSettings.cs
│   ├── PayrollSettings.cs
│   └── KioskSettings.cs
├── Controllers/             # 14 MVC controllers
│   ├── AccountController    # Authentication (Login, Logout, Profile)
│   ├── DashboardController  # Dashboard statistics
│   ├── EmployeeController   # Employee CRUD + Profile
│   ├── DepartmentController # Department management
│   ├── PositionController   # Position management
│   ├── AttendanceController # Time In/Out, Scanner, Adjust, Summary
│   ├── LeaveController      # Leave applications, credits, types
│   ├── DeductionController  # Employee loan/other deductions
│   ├── PayrollController    # Payroll computation, payslips
│   ├── ReportsController    # Report generation
│   ├── UsersController      # User account management
│   ├── SettingsController   # System settings
│   ├── AuditLogController   # Audit trail viewer
│   └── BaseController       # Shared helper methods
├── Data/
│   └── ApplicationDbContext # EF Core DbContext + seed data
├── Models/                  # 15 entity models
├── Services/                # 8 services
│   ├── AttendanceComputationService  # Late/OT/undertime computation
│   ├── TaxComputationService         # BIR TRAIN Law tax + gov't contributions
│   ├── LeaveCreditService            # Leave credit allocation & refresh
│   ├── AuditService                  # Audit log writer
│   ├── PayslipPdfService             # iText7 PDF payslip generation
│   ├── ExcelExportService            # ClosedXML Excel report export
│   ├── RecurringDeductionService     # Auto-generates deductions from schedules
│   └── LeaveCreditRefreshJob         # Background job: annual leave credit refresh
├── ViewModels/              # View models for all forms & displays
├── Views/                   # 55+ Razor views
│   ├── Account/
│   ├── Attendance/          # Index, Scanner, Adjust, Summary
│   ├── Deduction/           # Index, Create, Edit
│   ├── Dashboard/
│   ├── Employee/
│   ├── Leave/
│   ├── Payroll/
│   ├── Reports/
│   ├── Settings/
│   └── Shared/              # _Layout.cshtml, _ValidationScriptsPartial
└── wwwroot/
    ├── css/pecci-theme.css  # PECCI brand CSS theme
    ├── images/pecci-logo.png
    └── favicon.png          # Browser tab icon (PECCI logo)
```

### 3.2 Dependency Injection
All services are registered in `Program.cs`:
```csharp
// Scoped services (one instance per HTTP request)
builder.Services.AddScoped<AttendanceComputationService>();
builder.Services.AddScoped<TaxComputationService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<LeaveCreditService>();
builder.Services.AddScoped<PayslipPdfService>();
builder.Services.AddScoped<ExcelExportService>();
builder.Services.AddScoped<RecurringDeductionService>();

// Hosted background service (runs once after startup)
builder.Services.AddHostedService<LeaveCreditRefreshJob>();
```

### 3.3 Configuration Binding
Settings are bound to strongly-typed classes:
```csharp
builder.Services.Configure<AttendanceSettings>(
    builder.Configuration.GetSection("AttendanceSettings"));
builder.Services.Configure<PayrollSettings>(
    builder.Configuration.GetSection("PayrollSettings"));
builder.Services.Configure<KioskSettings>(
    builder.Configuration.GetSection("KioskSettings"));
```

### 3.4 System Architecture Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                        BROWSER                               │
│          Bootstrap 5.3 + PECCI Theme + Chart.js              │
└─────────────────────────┬────────────────────────────────────┘
                          │  HTTP / HTTPS
┌─────────────────────────▼────────────────────────────────────┐
│               ASP.NET Core MVC  (.NET 10)                    │
│                                                              │
│  ┌─────────────────┐  ┌──────────────────┐  ┌────────────┐  │
│  │   Controllers   │→ │    Services      │→ │ ViewModels │  │
│  │  (14 classes)   │  │  Attendance      │  │            │  │
│  │  Account        │  │  Tax             │  │            │  │
│  │  Dashboard      │  │  LeaveCredit     │  │            │  │
│  │  Employee       │  │  Audit           │  │            │  │
│  │  Attendance     │  └──────────────────┘  └────────────┘  │
│  │  Deduction      │                                         │
│  │  Leave          │  ┌──────────────────┐                   │
│  │  Payroll        │  │  Configuration   │                   │
│  │  Reports        │  │  AttendanceSet.  │                   │
│  │  Users          │  │  PayrollSettings │                   │
│  │  Settings       │  │  KioskSettings   │                   │
│  │  AuditLog       │  └──────────────────┘                   │
│  └─────────────────┘                                         │
└─────────────────────────┬────────────────────────────────────┘
                          │  Entity Framework Core 9
┌─────────────────────────▼────────────────────────────────────┐
│                  SQL Server / LocalDB                        │
│                                                              │
│  Users          Roles           RolePermissions              │
│  Employees      Departments     Positions                    │
│  AttendanceRecords              LeaveTypes                   │
│  LeaveApplications              LeaveCredits                 │
│  EmployeeDeductions             PayrollRecords               │
│  AuditLogs      SystemSettings                               │
└──────────────────────────────────────────────────────────────┘
```

### 3.5 Request Flow

```
Browser Request
      │
      ▼
[Cookie Auth Middleware]  ← validates session
      │
      ▼
[Route Matching]  → Controller.Action()
      │
      ▼
[Authorization Check]  ← [Authorize(Roles="...")]
      │
      ▼
[Controller Action]
      │
      ├── Queries DbContext (EF Core → SQL Server)
      ├── Calls Service (business logic)
      └── Returns View(viewModel) or RedirectToAction()
            │
            ▼
      [Razor View]  → HTML Response → Browser
```

---

## 4. Database Design

### 4.1 Entity Relationship Diagram

```
┌──────────┐       ┌──────────┐
│  Roles   │──────<│  Users   │
└──────────┘  1:N  └────┬─────┘
                        │ 0:1
                        ▼
┌─────────────┐    ┌────────────┐    ┌─────────────┐
│ Departments │──<─│  Employees │─>──│  Positions  │
└──────┬──────┘1:N └─────┬──────┘N:1 └─────────────┘
       │                 │
       └──────>──────────┘
       (Positions also belong to Departments)

Employees 1:N ──> AttendanceRecords
Employees 1:N ──> LeaveApplications ──> LeaveTypes
Employees 1:N ──> LeaveCredits      ──> LeaveTypes
Employees 1:N ──> PayrollRecords
Employees 1:N ──> EmployeeDeductions

Roles     1:N ──> RolePermissions
AuditLogs       (standalone — no FK)
SystemSettings  (standalone — no FK)
```

### 4.2 Table Descriptions

#### Users
| Column | Type | Description |
|---|---|---|
| UserID | int PK | Auto-increment primary key |
| Username | nvarchar(50) UNIQUE | Login username |
| PasswordHash | nvarchar(max) | BCrypt hashed password |
| Email | nvarchar(100) UNIQUE | User email |
| RoleID | int FK → Roles | Role assignment |
| EmployeeID | int FK → Employees nullable | Linked employee record |
| IsActive | bit | Account active/inactive |
| LastLogin | datetime2 nullable | Last successful login timestamp |
| CreatedAt | datetime2 | Account creation timestamp |

#### Employees
| Column | Type | Description |
|---|---|---|
| EmployeeID | int PK | Auto-increment |
| EmployeeNo | nvarchar(20) UNIQUE | e.g., EMP-0001 |
| FirstName | nvarchar(50) | First name |
| MiddleName | nvarchar(50) nullable | Middle name |
| LastName | nvarchar(50) | Last name |
| Suffix | nvarchar(10) nullable | Jr., Sr., III, etc. |
| DateOfBirth | datetime2 | Date of birth |
| Gender | nvarchar(10) | Male / Female |
| CivilStatus | nvarchar(20) nullable | Single / Married / etc. |
| Nationality | nvarchar(20) nullable | Default: Filipino |
| Address | nvarchar(300) nullable | Home address |
| ContactNumber | nvarchar(20) nullable | Philippine mobile format |
| PersonalEmail | nvarchar(100) nullable | Personal email |
| CompanyEmail | nvarchar(100) nullable | Work email |
| SSSNumber | nvarchar(20) nullable | Format: XX-XXXXXXX-X |
| PhilHealthNumber | nvarchar(20) nullable | Format: XX-XXXXXXXXX-X |
| PagIbigNumber | nvarchar(20) nullable | Format: XXXX-XXXX-XXXX |
| TINNumber | nvarchar(20) nullable | Format: XXX-XXX-XXX |
| DepartmentID | int FK → Departments | Department assignment |
| PositionID | int FK → Positions | Position assignment |
| DateHired | datetime2 | Employment start date |
| DateRegularized | datetime2 nullable | Regularization date |
| DateSeparated | datetime2 nullable | Separation date |
| EmploymentStatus | nvarchar(30) | Regular / Probationary / Contractual / Part-time |
| Status | nvarchar(20) | Active / Inactive / Resigned / Terminated / Retired |
| CreatedAt | datetime2 | Record creation timestamp |
| CreatedBy | int nullable | UserID who created the record |

#### AttendanceRecords
| Column | Type | Description |
|---|---|---|
| AttendanceID | int PK | Auto-increment |
| EmployeeID | int FK → Employees | Employee reference |
| AttendanceDate | datetime2 | Date of attendance |
| TimeIn | time nullable | Time-in (HH:mm:ss) |
| TimeOut | time nullable | Time-out (HH:mm:ss) |
| BreakOut | time nullable | Break start |
| BreakIn | time nullable | Break end |
| LateMinutes | float nullable | Computed late minutes |
| OvertimeMinutes | float nullable | Computed overtime minutes |
| UndertimeMinutes | float nullable | Computed undertime minutes |
| TotalHoursWorked | float nullable | Computed total hours |
| AttendanceStatus | nvarchar(20) | Present / Late / Absent / On Leave / Holiday |
| IsManualEntry | bit | True if HR-adjusted |
| Remarks | nvarchar(300) nullable | Adjustment notes |
| AdjustedBy | int nullable | UserID who adjusted |
| AdjustedAt | datetime2 nullable | Adjustment timestamp |
| CreatedAt | datetime2 | Record creation timestamp |

#### LeaveApplications
| Column | Type | Description |
|---|---|---|
| LeaveApplicationID | int PK | Auto-increment |
| EmployeeID | int FK → Employees | Applicant |
| LeaveTypeID | int FK → LeaveTypes | Type of leave |
| StartDate | datetime2 | Leave start date |
| EndDate | datetime2 | Leave end date |
| NumberOfDays | decimal(5,1) | Computed number of days |
| Reason | nvarchar(500) nullable | Reason for leave |
| Status | nvarchar(20) | Pending / Pending HR / Approved / Rejected / Cancelled |
| ManagerApproverID | int nullable | Manager who acted |
| ManagerApprovedAt | datetime2 nullable | Manager action timestamp |
| ManagerRemarks | nvarchar(300) nullable | Manager remarks |
| HRApproverID | int nullable | HR who acted |
| HRApprovedAt | datetime2 nullable | HR action timestamp |
| HRRemarks | nvarchar(300) nullable | HR remarks |
| AppliedAt | datetime2 | Application submission timestamp |

#### EmployeeDeductions *(new)*
| Column | Type | Description |
|---|---|---|
| DeductionID | int PK | Auto-increment |
| EmployeeID | int FK → Employees | Employee reference |
| DeductionType | nvarchar(50) | SSS Loan / Pag-IBIG Loan / PECCI Loan / Cash Advance / Other |
| Description | nvarchar(200) | Deduction description / reference number |
| Amount | decimal(18,2) | Deduction amount in PHP |
| CutoffPeriod | nvarchar(10) | 1-15 or 16-30 |
| Month | int | Month (1–12) |
| Year | int | Year (e.g., 2026) |
| Status | nvarchar(20) | Active / Applied / Cancelled |
| CreatedAt | datetime2 | Record creation timestamp |
| CreatedBy | int nullable | UserID who created the record |

> **Status lifecycle:** `Active` → payroll picks it up → `Applied`. HR can set to `Cancelled` before payroll runs.

#### PayrollRecords
| Column | Type | Description |
|---|---|---|
| PayrollID | int PK | Auto-increment |
| EmployeeID | int FK → Employees | Employee reference |
| PayPeriod | nvarchar(20) | e.g., "2026-04-1-15" |
| PeriodStart | datetime2 | Cutoff start date |
| PeriodEnd | datetime2 | Cutoff end date |
| BasicSalary | decimal(18,2) | Semi-monthly basic pay |
| OvertimePay | decimal(18,2) | Computed OT pay |
| HolidayPay | decimal(18,2) | Holiday pay |
| NightDifferential | decimal(18,2) | Night differential pay |
| Allowances | decimal(18,2) | Allowances |
| OtherEarnings | decimal(18,2) | Other earnings |
| SSSContribution | decimal(18,2) | SSS employee share |
| PhilHealthContribution | decimal(18,2) | PhilHealth employee share |
| PagIbigContribution | decimal(18,2) | Pag-IBIG employee share |
| WithholdingTax | decimal(18,2) | BIR withholding tax |
| LateDeductions | decimal(18,2) | Late deduction amount |
| UndertimeDeductions | decimal(18,2) | Undertime deduction amount |
| OtherDeductions | decimal(18,2) | Absent deductions + custom deductions |
| StoredGrossPay | decimal(18,2) | Snapshot of gross pay at compute time |
| StoredTotalDeductions | decimal(18,2) | Snapshot of total deductions |
| StoredNetPay | decimal(18,2) | Snapshot of net pay |
| WorkingDays | int | Working days in period |
| DaysWorked | int | Days employee actually worked |
| DaysAbsent | int | Days absent |
| TotalOvertimeHours | float | Total OT hours |
| TotalLateMinutes | float | Total late minutes |
| Status | nvarchar(20) | Draft / Finalized / Released |
| CreatedAt | datetime2 | Computation timestamp |
| FinalizedAt | datetime2 nullable | Finalization timestamp |

#### SystemSettings
| Column | Type | Description |
|---|---|---|
| SettingID | int PK | Auto-increment |
| SettingKey | nvarchar(100) | Setting identifier |
| SettingValue | nvarchar(max) | Current value |
| SettingGroup | nvarchar(50) | Attendance / Payroll / Tax / General |
| DataType | nvarchar(20) | string / int / decimal / bool / time |
| AllowedValues | nvarchar(200) nullable | Comma-separated allowed values |
| Description | nvarchar(200) nullable | Human-readable description |
| IsEditable | bit | Whether editable from UI |
| UpdatedAt | datetime2 | Last update timestamp |
| UpdatedBy | int nullable | UserID who last updated |

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
| Adjust (GET) | GET | HR Admin, HR Staff | Manual adjustment form |
| Adjust (POST) | POST | HR Admin, HR Staff | Save adjustment + recompute |
| Summary | GET | HR Admin, HR Staff | Monthly summary report |
| Scanner | GET | Anonymous | Barcode/ID scanner kiosk terminal |
| Scan | POST | Anonymous | Process barcode scan (JSON API) |

**Computation Logic (AttendanceComputationService):**
- Late threshold = WorkStart + GracePeriodMinutes + GracePeriodSeconds
- Late minutes = ceil(TimeIn − WorkStart) if TimeIn > LateThreshold
- Overtime = floor(TimeOut − WorkEnd) if TimeOut > WorkEnd AND minutes ≥ OvertimeThreshold
- Undertime = ceil(WorkEnd − TimeOut) if TimeOut < WorkEnd
- Hours worked = (TimeOut − TimeIn) − LunchBreakDuration (if worked through lunch)

**Scanner / Kiosk Terminal:**
The `/Attendance/Scanner` page is a standalone kiosk display (no login required) that accepts barcode or RFID scans. It supports:
- Lookup by **Employee No** (exact match)
- Lookup by **Display Name** ("FirstName LastName")
- Lookup by **Full Name** ("LastName, FirstName MiddleName")
- Partial name match (first or last name, if unique)
- **Double-scan confirmation** — first scan shows a pending confirmation, second scan within 10 seconds commits the Time In or Time Out

```
Scan Flow:
  1st scan → "CONFIRM TIME IN — scan again within 10s"
  2nd scan → TIME IN recorded ✅

  3rd scan → "CONFIRM TIME OUT — scan again within 10s"
  4th scan → TIME OUT recorded ✅
```

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

### 5.5 Deductions Module *(new)*

**Controller:** `DeductionController`

| Action | Method | Access | Description |
|---|---|---|---|
| Index | GET | HR Admin, HR Staff | List deductions with filters |
| Create (GET) | GET | HR Admin, HR Staff | Add deduction form |
| Create (POST) | POST | HR Admin, HR Staff | Save new deduction |
| Edit (GET) | GET | HR Admin, HR Staff | Edit deduction form |
| Edit (POST) | POST | HR Admin, HR Staff | Update deduction |
| Cancel | POST | HR Admin, HR Staff | Cancel a deduction |

**Deduction Types:**
- SSS Loan
- Pag-IBIG Loan
- PECCI Loan
- Cash Advance
- Other

**Integration with Payroll:**
Deductions with `Status = "Active"` matching the employee, month, year, and cutoff period are automatically picked up during payroll computation and added to `OtherDeductions`. After payroll is computed, their status is set to `"Applied"`.

**Status Lifecycle:**
```
HR adds deduction → Status: Active
      │
      ▼
Payroll computed → Status: Applied  (auto)
      OR
HR cancels → Status: Cancelled  (manual, before payroll runs)
```

### 5.6 Payroll Module (Sprint 4)

**Controller:** `PayrollController`
**Services:** `TaxComputationService`, `AttendanceComputationService`

| Action | Method | Access | Description |
|---|---|---|---|
| Index | GET | HR Admin, HR Staff | Payroll records list |
| Compute (GET) | GET | HR Admin, HR Staff | Computation parameters form |
| Compute (POST) | POST | HR Admin, HR Staff | Run payroll computation |
| Payslips | GET | HR Admin, HR Staff | View payslips |
| Finalize | POST | HR Admin | Finalize payroll record |

**Full Computation Steps:**
1. Determine period dates from cutoff selection (1–15 or 16–30)
2. Get employee's position `BasicSalary` ÷ 2 (semi-monthly)
3. Retrieve `AttendanceRecords` for the period
4. Compute: overtime pay, late deductions, **undertime deductions**, absent deductions
5. Fetch `EmployeeDeductions` (Active, matching month/year/cutoff) → sum as custom deductions
6. Compute government contributions (SSS, PhilHealth, Pag-IBIG) on monthly basis ÷ 2
7. Compute withholding tax on monthly taxable income ÷ 2
8. Store `StoredGrossPay`, `StoredTotalDeductions`, `StoredNetPay` as snapshots
9. Mark fetched `EmployeeDeductions` as `"Applied"`

**Payroll Computation Flow:**
```
Select Cutoff Period + Month + Year
          │
          ▼
For each Active Employee:
  ├── Get BasicSalary from Position (÷2)
  ├── Get AttendanceRecords for period
  │     ├── Sum OvertimeMinutes → OvertimePay
  │     ├── Sum LateMinutes → LateDeduction
  │     ├── Sum UndertimeMinutes → UndertimeDeduction
  │     └── Count DaysAbsent → AbsentDeduction
  ├── Get EmployeeDeductions (Active, same period)
  │     └── Sum Amount → CustomDeductions
  ├── Compute Gov't Contributions (monthly ÷2)
  │     ├── SSS (4.5% of MSC)
  │     ├── PhilHealth (2.5% of basic)
  │     └── Pag-IBIG (2%, max ₱100)
  ├── Compute Withholding Tax (BIR TRAIN Law ÷12)
  ├── OtherDeductions = AbsentDeduction + CustomDeductions
  ├── Store PayrollRecord (Status: Draft)
  └── Mark EmployeeDeductions → Applied
```

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
| Dashboard | ✅ Full | ✅ Full | ✅ Full | ✅ Own stats |
| Employee List | ✅ Full | ✅ Full | ✅ View | ✅ Own profile |
| Employee CRUD | ✅ Full | ✅ Create/Edit | ❌ | ❌ |
| Employee Deactivate | ✅ | ❌ | ❌ | ❌ |
| Departments | ✅ Full | ✅ Create/Edit | ❌ | ❌ |
| Positions | ✅ Full | ✅ Create/Edit | ❌ | ❌ |
| Attendance View | ✅ All | ✅ All | ✅ View | ✅ Own |
| Attendance Adjust | ✅ | ✅ | ❌ | ❌ |
| Scanner Terminal | ✅ | ✅ | ✅ | ✅ (anonymous) |
| Leave Apply | ✅ | ✅ | ✅ | ✅ |
| Leave Approve | ✅ | ✅ | ✅ (Manager step) | ❌ |
| Leave Types | ✅ Full | ✅ View | ❌ | ❌ |
| Deductions | ✅ Full | ✅ Full | ❌ | ❌ |
| Payroll Compute | ✅ | ✅ | ❌ | ❌ |
| Payroll Finalize | ✅ | ❌ | ❌ | ❌ |
| Payslips | ✅ | ✅ | ❌ | ❌ |
| Reports | ✅ All | ✅ All | ✅ All | ❌ |
| User Management | ✅ | ❌ | ❌ | ❌ |
| System Settings | ✅ | ❌ | ❌ | ❌ |
| Audit Trail | ✅ | ❌ | ❌ | ❌ |

---

## 11. API Reference

### AJAX / JSON Endpoints

#### GET /Employee/GetPositionsByDepartment
Returns positions for a given department (used in employee form dropdowns).

**Parameters:** `departmentId` (int)

**Response:**
```json
[
  { "positionID": 1, "positionTitle": "HR Manager", "basicSalary": 65000.00 }
]
```

---

#### POST /Attendance/Scan
Processes a barcode or RFID scan from the kiosk terminal. Anonymous — no login required.

**Request Body:**
```json
{ "employeeNo": "EMP-0001", "deviceIP": "192.168.1.10" }
```
> `employeeNo` can be an Employee No, Display Name ("Juan Dela Cruz"), or Full Name ("Dela Cruz, Juan").

**Response — Pending (first scan):**
```json
{
  "success": true,
  "action": "CONFIRM TIME IN",
  "employeeNo": "EMP-0001",
  "employeeName": "Juan Dela Cruz",
  "department": "Human Resources",
  "timeRecorded": "08:02:15",
  "message": "Scan again within 10s to confirm TIME IN.",
  "isLate": false,
  "isPending": true
}
```

**Response — Confirmed (second scan):**
```json
{
  "success": true,
  "action": "TIME IN",
  "employeeNo": "EMP-0001",
  "employeeName": "Juan Dela Cruz",
  "department": "Human Resources",
  "timeRecorded": "08:02:18",
  "message": "On time",
  "isLate": false,
  "isPending": false
}
```

---

#### POST /Settings/UpdateSetting
Updates a single system setting. Requires HR Admin role.

**Request Body:**
```json
{ "settingID": 3, "settingKey": "GracePeriodMinutes", "settingValue": "10" }
```

**Response:**
```json
{ "success": true, "message": "GracePeriodMinutes updated successfully." }
```

---

#### GET /Settings/PreviewAttendanceRule
Returns computed late threshold for live preview in the Settings UI.

**Parameters:** `workStart` (HH:mm), `workEnd` (HH:mm), `graceMins` (int), `graceSecs` (int), `otThreshold` (int)

**Response:**
```json
{
  "success": true,
  "lateThreshold": "08:10:00",
  "description": "Employees can time-in up to 08:10:00. Any time-in after that is marked LATE."
}
```

---

## 12. Known Issues & Limitations

### Fully implemented
1. **PDF Payslip Export** — iText7 generates single and multi-page PDFs. Download buttons on the Payslips page (`DownloadPdf`, `DownloadAllPdf`). Includes PECCI logo watermark.
2. **Excel Report Export** — ClosedXML exports all 4 report types (Employee List, Attendance Summary, Leave Summary, Payroll Summary). Export buttons on each report page.
3. **Recurring Deductions** — `RecurringDeductionSchedule` model and `RecurringDeductionService` implemented. Schedules auto-generate `EmployeeDeduction` entries per cutoff. UI for managing schedules is planned.
4. **Leave Credit Background Refresh** — `LeaveCreditRefreshJob` runs 5 seconds after startup. Only allocates if no credits exist for the current year — no performance impact on normal restarts.

### Not yet implemented
5. **Recurring Deduction UI** — The `RecurringDeductionService` and model are complete but there is no controller or views for HR to manage recurring schedules. Must be done via direct DB entry for now.
6. **Email Notifications** — Leave approval/rejection notifications via email are not in this version.
7. **Biometric Device Integration** — Time In/Out is web-based. The Scanner Terminal supports barcode/RFID via keyboard-wedge scanners. Native biometric device SDK integration is out of scope.
8. **Holiday Calendar** — Regular and special Philippine holidays must be manually marked in attendance records. An automated holiday calendar (based on Proclamation list) is planned.
9. **Night Differential Auto-Computation** — Night differential rate is defined in settings but not automatically applied during payroll. Manual entry via `OtherEarnings` is required for now.

---

*Document prepared by UST Interns: Arkin Reinier Aguilar, Maxenne De Guzman, Bernice Elyssa Soriano, Emily Etea*
*© 2026 PECCI Multipurpose Cooperative — All rights reserved*
*Repository: https://github.com/EmilyEtea/PECCI-HRIS*
