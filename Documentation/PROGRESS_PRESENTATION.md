# PECCI HRIS — Sprint Progress Presentation
## PLDT Employees Credit Cooperative, Inc.
### Human Resource Information System

---

> **Presented by:** UST Interns
> **Arkin Reinier Aguilar | Maxenne De Guzman | Bernice Elyssa Soriano | Emily Etea**
> **Date:** 2026
> **Internship Program:** University of Santo Tomas (UST)

---

## SLIDE 1 — Title Slide

```
╔══════════════════════════════════════════════════════════╗
║                                                          ║
║          🟢  PECCI HRIS  🟢                              ║
║     Human Resource Information System                    ║
║                                                          ║
║   PLDT Employees Credit Cooperative, Inc.                ║
║                                                          ║
║   ─────────────────────────────────────────              ║
║   Developed by UST Interns:                              ║
║   Arkin Reinier Aguilar                                  ║
║   Maxenne De Guzman                                      ║
║   Bernice Elyssa Soriano                                 ║
║   Emily Etea                                             ║
║                                                          ║
║   2026                                                   ║
╚══════════════════════════════════════════════════════════╝
```

---

## SLIDE 2 — Project Background

### What is PECCI HRIS?

A **web-based Human Resource Information System** developed for PLDT Employees Credit Cooperative, Inc. (PECCI) to:

- ✅ Centralize employee records
- ✅ Automate attendance tracking
- ✅ Streamline leave management
- ✅ Automate payroll computation
- ✅ Ensure Philippine regulatory compliance

### The Problem We Solved
> Manual HR processes → fragmented data → inefficiency → errors

### Our Solution
> Integrated, web-based HRIS with role-based access, automated computations, and real-time reporting

---

## SLIDE 3 — Technology Stack

| Layer | Technology |
|---|---|
| **Backend** | ASP.NET Core MVC (.NET 10) |
| **Database** | SQL Server / LocalDB |
| **ORM** | Entity Framework Core 9 |
| **Frontend** | Bootstrap 5.3 + PECCI Custom Theme |
| **Icons** | Font Awesome 6.5 |
| **Charts** | Chart.js |
| **Security** | BCrypt + Cookie Auth |
| **Version Control** | GitHub |

### Brand Colors
- 🟢 **Primary Green** `#2E7D32` — PECCI logo green
- 🟠 **Accent Orange** `#E8521A` — PECCI logo orange
- ⬜ **White** `#FFFFFF` — Clean backgrounds

---

## SLIDE 4 — Sprint Overview

```
Sprint 0  ████████████████████  DONE  Planning & Setup
Sprint 1  ████████████████████  DONE  Auth & Employee Management
Sprint 2  ████████████████████  DONE  Attendance & Timekeeping
Sprint 3  ████████████████████  DONE  Leave Management
Sprint 4  ████████████████████  DONE  Payroll Integration
Sprint 5  ████████████████████  DONE  Reports & Optimization
```

**Overall Progress: 100% of planned features implemented**

---

## SLIDE 5 — Sprint 1: Authentication & Employee Management ✅

### What We Built

**Authentication System**
- Secure login with BCrypt password hashing
- Role-based access: HR Admin, HR Staff, Manager, Employee
- Session management (8-hour timeout)
- Audit logging of all login attempts

**Employee Management**
- Complete CRUD operations (Create, Read, Update, Deactivate)
- Employee profile with personal info, government IDs, employment details
- Department and Position management
- Auto-generated employee numbers
- Auto-allocation of leave credits on hire

### Key Feature: Role-Based Access
```
HR Admin  → Full system access
HR Staff  → HR operations (no user management, no settings)
Manager   → View employees, approve leaves
Employee  → Own records only
```

---

## SLIDE 6 — Sprint 2: Attendance & Timekeeping ✅

### What We Built

**Time In / Time Out System**
- Web-based time recording with live clock
- Real-time late detection

**Philippine-Compliant Grace Period Logic**
```
Work Start:     08:00
Grace Period:   +5 minutes
Late Threshold: 08:05:00

✅ 08:05:00 → ON TIME
❌ 08:05:01 → LATE (6 minutes from 08:00)
```

**Automatic Computations**
- Late minutes (counted from 08:00, not from grace period end)
- Overtime (credited only if ≥ 30 minutes beyond work end)
- Undertime (left before official end time)
- Total hours worked (minus lunch break)

**Manual Adjustment**
- HR can adjust any attendance record
- System recomputes all values automatically
- All adjustments logged in audit trail

**Fully Adjustable Settings**
- All rules configurable from Admin → Settings UI
- No code changes needed

---

## SLIDE 7 — Sprint 3: Leave Management ✅

### What We Built

**Leave Types (Pre-configured)**
| Code | Type | Days/Year |
|---|---|---|
| VL | Vacation Leave | 15 |
| SL | Sick Leave | 15 |
| EL | Emergency Leave | 3 |
| ML | Maternity Leave | 105 |
| PL | Paternity Leave | 7 |

**Leave Application Workflow**
```
Employee Applies
      ↓
  Status: PENDING
      ↓
Manager Reviews → Approve/Reject
      ↓
  Status: PENDING HR
      ↓
HR Reviews → Approve/Reject
      ↓
  Status: APPROVED / REJECTED
      ↓
Credits automatically deducted
```

**Leave Credits Tracking**
- Real-time balance display
- Pending credits reserved during application
- Auto-deduction on approval
- Auto-release on rejection/cancellation

---

## SLIDE 8 — Sprint 4: Payroll Integration ✅

### What We Built

**BIR TRAIN Law Compliance (RA 10963)**
- Tax computation per RR 2-2023 (effective January 1, 2023)
- Automatic tax bracket identification
- Monthly withholding tax ÷ 2 for semi-monthly payroll

**Government Contributions**
| Contribution | Rate | Cap |
|---|---|---|
| SSS | 4.5% of MSC | ₱30,000 MSC max |
| PhilHealth | 2.5% of basic | ₱500–₱5,000/month |
| Pag-IBIG | 2% | ₱100/month max |

**Payroll Computation**
- Semi-monthly cutoffs (1st–15th, 16th–30th)
- Integrates attendance data (OT, late, absent)
- Generates payslips with full earnings/deductions breakdown
- Finalization workflow (Draft → Finalized)

**Sample Payslip Breakdown**
```
EARNINGS                    DEDUCTIONS
─────────────────────────   ─────────────────────────
Basic Salary:  ₱17,500.00   SSS:           ₱  675.00
Overtime Pay:  ₱    250.00  PhilHealth:    ₱  437.50
               ─────────    Pag-IBIG:      ₱  100.00
Gross Pay:     ₱17,750.00   Withholding:   ₱  500.00
                            Late Deduct:   ₱   45.00
                            ─────────────────────────
                            Total Deduct:  ₱1,757.50

                NET PAY:    ₱15,992.50
```

---

## SLIDE 9 — Sprint 5: Reports & Optimization ✅

### What We Built

**4 Report Types**
1. **Employee List** — Filterable by department and status, printable
2. **Attendance Summary** — Monthly per-employee with totals row
3. **Leave Summary** — Annual leave utilization by employee and type
4. **Payroll Summary** — Monthly payroll with gross/deductions/net totals

**Dashboard**
- Real-time statistics (present today, late, on leave, pending leaves)
- Attendance donut chart
- Recent activity feed
- Upcoming birthdays widget

**Audit Trail**
- All user actions logged (login, create, update, delete)
- Filterable by module, action, username, date range
- Color-coded rows (green=login, red=delete, yellow=update)

**System Settings UI**
- All attendance and payroll rules adjustable from UI
- Live preview of late threshold when changing grace period
- No code changes or server restart needed

---

## SLIDE 10 — System Architecture

```
┌─────────────────────────────────────────────────────┐
│                    BROWSER                          │
│         Bootstrap 5 + PECCI Theme + Chart.js        │
└──────────────────────┬──────────────────────────────┘
                       │ HTTPS
┌──────────────────────▼──────────────────────────────┐
│              ASP.NET Core MVC (.NET 10)             │
│                                                     │
│  Controllers (13)  →  Services (3)  →  ViewModels  │
│  ├── Account           ├── AttendanceComputation    │
│  ├── Dashboard         ├── TaxComputation           │
│  ├── Employee          └── Audit                   │
│  ├── Attendance                                     │
│  ├── Leave             Configuration               │
│  ├── Payroll           ├── AttendanceSettings       │
│  ├── Reports           └── PayrollSettings          │
│  ├── Users                                          │
│  ├── Settings                                       │
│  └── AuditLog                                       │
└──────────────────────┬──────────────────────────────┘
                       │ EF Core 9
┌──────────────────────▼──────────────────────────────┐
│              SQL Server / LocalDB                   │
│                                                     │
│  13 Tables: Users, Roles, Employees, Departments,  │
│  Positions, AttendanceRecords, LeaveTypes,          │
│  LeaveApplications, LeaveCredits, PayrollRecords,  │
│  AuditLogs, SystemSettings, RolePermissions        │
└─────────────────────────────────────────────────────┘
```

---

## SLIDE 11 — Key Technical Highlights

### 1. Configurable Grace Period
```csharp
// AttendanceSettings.cs
public TimeSpan LateThreshold =>
    WorkStart
    .Add(TimeSpan.FromMinutes(GracePeriodMinutes))
    .Add(TimeSpan.FromSeconds(GracePeriodSeconds));
```
All values stored in database — adjustable from UI without code changes.

### 2. BIR TRAIN Law Tax Engine
```csharp
// TaxComputationService.cs — 2023 brackets
if (annual <= 250_000m)       annualTax = 0m;
else if (annual <= 400_000m)  annualTax = (annual - 250_000m) * 0.15m;
else if (annual <= 800_000m)  annualTax = 22_500m + (annual - 400_000m) * 0.20m;
// ... etc.
return Math.Round(annualTax / 12m, 2); // Monthly
```

### 3. Audit Trail on Every Action
```csharp
await _auditService.LogAsync(
    userId, username, "Create", "Employee",
    $"Created employee {employee.FullName}",
    clientIP
);
```

### 4. Auto-Recompute on Adjustment
When HR adjusts attendance, the system automatically recomputes:
- Late minutes, overtime, undertime, total hours
- All values updated in a single call to `_computeService.Compute(record)`

---

## SLIDE 12 — Demo Walkthrough

### Login
- URL: `https://localhost:5001`
- Admin: `admin` / `Admin@123`

### Key Flows to Demonstrate

**1. Add Employee**
- Employees → Add Employee
- Select Department → Positions auto-load
- Salary auto-fills from position
- Leave credits auto-allocated

**2. Time In / Time Out**
- Attendance → Time In button
- System shows if late with minutes
- Time Out shows total hours + overtime

**3. Apply for Leave**
- Leave → Apply for Leave
- Shows remaining credits per type
- Live day counter (excludes weekends)

**4. Compute Payroll**
- Payroll → Compute Payroll
- Select cutoff period, month, year
- System pulls attendance data automatically
- View payslip with full breakdown

**5. System Settings**
- Settings → Change grace period
- Live preview shows new late threshold
- Save → takes effect immediately

---

## SLIDE 13 — Challenges & Solutions

| Challenge | Solution |
|---|---|
| SQL Server service not starting | Used LocalDB for development |
| .NET version mismatch (8 vs 10) | Upgraded project to .NET 10, EF Core 9 |
| Grace period logic complexity | Separated threshold from deduction counting |
| BIR tax bracket accuracy | Implemented annual-to-monthly conversion |
| Settings adjustable without restart | Stored in DB, loaded per-request |
| Audit trail performance | Scoped service, async logging |

---

## SLIDE 14 — What's Next (Future Enhancements)

1. **PDF Payslip Export** — Generate downloadable PDF payslips
2. **Excel Report Export** — Export all reports to Excel
3. **Email Notifications** — Notify employees on leave approval/rejection
4. **Biometric Integration** — Connect to fingerprint/face recognition devices
5. **Holiday Calendar** — Automatic Philippine holiday detection
6. **Mobile Responsive** — Enhanced mobile experience
7. **Dashboard Customization** — Drag-and-drop widgets
8. **Bulk Operations** — Bulk leave credit allocation, bulk payroll finalization

---

## SLIDE 15 — Conclusion

### What We Delivered

✅ **Sprint 1** — Secure authentication + complete employee management
✅ **Sprint 2** — Philippine-compliant attendance with configurable rules
✅ **Sprint 3** — Full leave management with approval workflow
✅ **Sprint 4** — BIR TRAIN Law compliant payroll computation
✅ **Sprint 5** — Reports, audit trail, dashboard, system settings

### Impact
- **Eliminates manual HR processes** for PECCI
- **Ensures regulatory compliance** (BIR, DOLE, SSS, PhilHealth, Pag-IBIG)
- **Reduces computation errors** through automated calculations
- **Provides full audit trail** for accountability
- **Fully configurable** — rules can be adjusted without developer involvement

---

## SLIDE 16 — Thank You

```
╔══════════════════════════════════════════════════════════╗
║                                                          ║
║          Thank You!                                      ║
║                                                          ║
║   PECCI HRIS v1.0.0                                      ║
║   PLDT Employees Credit Cooperative, Inc.                ║
║                                                          ║
║   ─────────────────────────────────────────              ║
║   Developed by UST Interns:                              ║
║                                                          ║
║   🎓 Arkin Reinier Aguilar                               ║
║   🎓 Maxenne De Guzman                                   ║
║   🎓 Bernice Elyssa Soriano                              ║
║   🎓 Emily Etea                                          ║
║                                                          ║
║   University of Santo Tomas (UST)                        ║
║   © 2026 PECCI — All rights reserved                     ║
╚══════════════════════════════════════════════════════════╝
```

---

*Repository: https://github.com/EmilyEtea/PECCI-HRIS*
