# PECCI HRIS — Human Resource Information System

**PECCI Multipurpose Cooperative**

A comprehensive web-based HRIS built with **ASP.NET Core MVC (.NET 10)**, Entity Framework Core 9, and SQL Server. Covers the full employee lifecycle — from onboarding to payroll — with full Philippine regulatory compliance (BIR TRAIN Law, DOLE, SSS, PhilHealth, Pag-IBIG).

> **Live at:** http://localhost:5000 (local dev)
> **Repository:** https://github.com/EmilyEtea/PECCI-HRIS

---

## 🎓 Development Team

**UST Interns — 2026**

| Name | Role |
|---|---|
| Arkin Reinier Aguilar | Developer |
| Maxenne De Guzman | Developer |
| Bernice Elyssa Soriano | Developer |
| Emily Etea | Developer |

---

## ✨ Features

### Authentication & Access Control
- ✅ Secure login with BCrypt password hashing
- ✅ Role-based access: **HR Admin, HR Staff, Manager, Employee**
- ✅ 8-hour session timeout (30-day "Remember Me" option)
- ✅ Audit trail — every action logged with IP address

### Employee Management
- ✅ Full CRUD — create, view, edit, deactivate employees
- ✅ Employee profile with personal info, government IDs (SSS, PhilHealth, Pag-IBIG, TIN)
- ✅ Department & Position management
- ✅ Auto-generated employee numbers (EMP-XXXX)
- ✅ Leave credits auto-allocated on hire

### Attendance & Timekeeping
- ✅ Web-based Time In / Time Out
- ✅ **Barcode / RFID Scanner Terminal** — standalone kiosk page, no login required
  - Lookup by Employee No, Display Name, or Full Name
  - Double-scan confirmation (scan twice within 10s to confirm)
- ✅ **Philippine-compliant grace period logic**
  - 08:00 start + 5 min grace = 08:05:00 cutoff
  - 08:05:01 → LATE (minutes counted from 08:00, not from grace period end)
- ✅ Overtime tracking (credited only if ≥ 30 min beyond work end)
- ✅ Undertime computation
- ✅ Manual attendance adjustment (HR only) with auto-recompute
- ✅ Monthly attendance summary report
- ✅ All rules fully configurable from Settings UI

### Leave Management
- ✅ 5 pre-configured leave types (VL, SL, Emergency, Maternity, Paternity)
- ✅ Leave application with **two-step approval workflow** (Manager → HR)
- ✅ Real-time leave credit balance tracking
- ✅ Pending credits reserved during application; released on rejection/cancellation
- ✅ Annual leave credit refresh (auto on startup, manual from admin panel)

### Deductions
- ✅ Add custom deductions per employee per cutoff period
- ✅ Types: SSS Loan, Pag-IBIG Loan, PECCI Loan, Cash Advance, Other
- ✅ **Automatically picked up during payroll computation**
- ✅ Status lifecycle: Active → Applied (auto) / Cancelled (manual)

### Payroll
- ✅ **BIR TRAIN Law (RA 10963) tax computation** — RR 2-2023 (2023 onwards)
- ✅ **SSS** (4.5% of MSC, 2026 table, ₱4,000–₱30,000 MSC range)
- ✅ **PhilHealth** (2.5% employee share, ₱500–₱5,000/month cap)
- ✅ **Pag-IBIG** (2%, max ₱100/month)
- ✅ Overtime pay (DOLE rates: 1.25× regular, 1.30× rest day, 2.0× holiday)
- ✅ Late & undertime deductions (auto-computed from daily rate)
- ✅ Absent deductions (daily rate × days absent)
- ✅ Custom deductions from Deductions module integrated automatically
- ✅ Semi-monthly cutoffs (1st–15th, 16th–30th)
- ✅ Payslip view with full earnings/deductions breakdown
- ✅ Finalization workflow (Draft → Finalized)

### Reports
- ✅ Employee List report (filterable by department/status)
- ✅ Attendance Summary report (monthly, per employee)
- ✅ Leave Summary report (annual)
- ✅ Payroll Summary report (monthly totals)
- ✅ All reports are print-ready

### Administration
- ✅ **System Settings UI** — adjust all rules without touching code or restarting
- ✅ User management (create, edit, reset password, activate/deactivate)
- ✅ Audit trail viewer (filterable by module, action, user, date)
- ✅ Dashboard with real-time stats, attendance chart, upcoming birthdays

---

## 🛠️ Technology Stack

| Layer | Technology | Version |
|---|---|---|
| Backend | ASP.NET Core MVC | .NET 10.0 |
| ORM | Entity Framework Core | 9.0.4 |
| Database | SQL Server / LocalDB | 2019+ |
| Auth | Cookie-based + BCrypt | Built-in / 4.0.3 |
| PDF | iText7 | 8.0.4 |
| Excel | ClosedXML | 0.104.2 |
| Frontend | Bootstrap | 5.3.3 |
| Icons | Font Awesome | 6.5.0 |
| Charts | Chart.js | 4.4.2 |
| Version Control | Git / GitHub | — |

---

## 📦 Installation & Setup

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- SQL Server 2019+ or SQL Server Express (LocalDB works out of the box)
- Visual Studio 2022 or VS Code with C# Dev Kit

### Step 1 — Clone
```bash
git clone https://github.com/EmilyEtea/PECCI-HRIS.git
cd PECCI-HRIS/PECCI_HRIS
```

### Step 2 — Configure connection string
Edit `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=PECCI_HRIS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**Other common formats:**
| Scenario | Connection String |
|---|---|
| SQL Express | `Server=localhost\\SQLEXPRESS;Database=PECCI_HRIS_DB;Trusted_Connection=True;TrustServerCertificate=True;` |
| Named instance | `Server=YOUR_PC\\SQLEXPRESS;Database=PECCI_HRIS_DB;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Auth | `Server=localhost;Database=PECCI_HRIS_DB;User Id=sa;Password=YourPwd;TrustServerCertificate=True;` |

### Step 3 — Restore packages
```bash
dotnet restore
```

### Step 4 — Apply migrations
```bash
dotnet ef database update
```
This creates the database, all 14 tables, and seeds default data (roles, departments, leave types, system settings).

### Step 5 — Run
```bash
dotnet run
```
Navigate to **http://localhost:5000**

### Step 6 — Login
| Field | Value |
|---|---|
| Username | `admin` |
| Password | `Admin@123` |

> ⚠️ Change the admin password immediately after first login.

---

## ⚙️ Adjustable Settings

All rules are stored in the database and editable from **Admin → System Settings** — no code changes or restarts needed.

### Attendance
| Setting | Default | Description |
|---|---|---|
| WorkStartTime | 08:00 | Official work start |
| WorkEndTime | 17:00 | Official work end |
| GracePeriodMinutes | 5 | 08:00 + 5 = 08:05 cutoff |
| GracePeriodSeconds | 0 | Additional seconds |
| OvertimeThresholdMinutes | 30 | Min OT before pay credited |
| LateDeductionType | PerMinute | PerMinute / PerHour / FixedAmount / None |
| LateDeductionAmountPerMinute | 0 | 0 = auto from daily rate |
| UndertimeDeductionType | PerMinute | Same options as late |

### Payroll
| Setting | Default | Description |
|---|---|---|
| CutoffDay1 | 15 | First cutoff day |
| CutoffDay2 | 30 | Second cutoff day |
| OvertimeRateMultiplier | 1.25 | Regular OT (DOLE: 125%) |
| RestDayOvertimeRateMultiplier | 1.30 | Rest day OT (DOLE: 130%) |
| RegularHolidayRateMultiplier | 2.00 | Regular holiday (DOLE: 200%) |
| NightDifferentialRate | 0.10 | Night diff (DOLE: 10%) |

### Tax & Government Contributions
| Setting | Default | Description |
|---|---|---|
| TaxTableType | BIR_TRAIN_LAW_2023 | BIR withholding tax table |
| SSSEmployeeRate | 0.045 | SSS employee share (4.5%) |
| PhilHealthRate | 0.025 | PhilHealth employee share (2.5%) |
| PagIbigRate | 0.02 | Pag-IBIG rate (2%) |
| PagIbigMaxContribution | 100.00 | Max Pag-IBIG/month |

---

## 📊 BIR TRAIN Law Tax Table (RR 2-2023, effective Jan 1, 2023)

| Annual Taxable Income | Tax |
|---|---|
| Up to ₱250,000 | **Exempt** |
| ₱250,001 – ₱400,000 | 15% of excess over ₱250,000 |
| ₱400,001 – ₱800,000 | ₱22,500 + 20% of excess over ₱400,000 |
| ₱800,001 – ₱2,000,000 | ₱102,500 + 25% of excess over ₱800,000 |
| ₱2,000,001 – ₱8,000,000 | ₱402,500 + 30% of excess over ₱2,000,000 |
| Over ₱8,000,000 | ₱2,202,500 + 35% of excess over ₱8,000,000 |

Monthly withholding tax = Annual tax ÷ 12

---

## 📁 Project Structure

```
PECCI_HRIS/
├── Configuration/          # AttendanceSettings, PayrollSettings, KioskSettings
├── Controllers/            # 14 MVC controllers
├── Data/                   # ApplicationDbContext + migrations
├── Database/               # SQL scripts & setup guide
├── Documentation/          # Technical documentation
├── Models/                 # 14 entity models
├── Services/               # AttendanceComputation, Tax, LeaveCredit, Audit
├── ViewModels/             # View models for all forms & displays
├── Views/                  # 55+ Razor views
│   ├── Account/            # Login, Profile, AccessDenied
│   ├── Attendance/         # Index, Scanner, Adjust, Summary
│   ├── Deduction/          # Index, Create, Edit
│   ├── Dashboard/
│   ├── Employee/
│   ├── Leave/
│   ├── Payroll/
│   ├── Reports/
│   ├── Settings/
│   └── Shared/             # _Layout.cshtml
├── wwwroot/
│   ├── css/pecci-theme.css # PECCI brand theme
│   ├── images/pecci-logo.png
│   └── favicon.png         # Browser tab icon
├── appsettings.json
├── Program.cs
└── HRIS.sln
```

---

## 🔒 Security

- ✅ BCrypt password hashing (cost factor 11)
- ✅ Role-based authorization on all controllers/actions
- ✅ Anti-forgery tokens on all POST forms
- ✅ EF Core parameterized queries (SQL injection protection)
- ✅ Audit trail with IP address logging
- ✅ 8-hour session timeout with sliding expiration
- ✅ HttpOnly cookies

---

## 📝 Credits

Developed by **UST Interns** for **PECCI Multipurpose Cooperative**
University of Santo Tomas (UST) Internship Program — 2026

---

**© 2026 PECCI Multipurpose Cooperative — All rights reserved.**

*Compliant with: BIR TRAIN Law (RA 10963, RR 2-2023) · DOLE Labor Code · SSS · PhilHealth · Pag-IBIG (as of 2026)*
