# PECCI HRIS — Human Resource Information System

**Multipurpose Cooperative**

A comprehensive web-based HRIS built with ASP.NET Core 8.0, Entity Framework Core, and SQL Server. Features employee management, attendance tracking with Philippine labor law compliance, leave management, and payroll computation based on BIR TRAIN Law.

---

## 🎓 Development Team

**UST Interns:**
- **Arkin Reinier Aguilar**
- **Maxenne De Guzman**
- **Bernice Elyssa Soriano**
- **Emily Etea**

---

## ✨ Features

### Sprint 1 — Authentication & Employee Management
- ✅ Role-based authentication (HR Admin, HR Staff, Manager, Employee)
- ✅ Password hashing with BCrypt
- ✅ Employee CRUD operations
- ✅ Department & Position management
- ✅ Employee profile with government IDs

### Sprint 2 — Attendance & Timekeeping
- ✅ Time In / Time Out functionality
- ✅ **Philippine-compliant grace period** (8:00 + 5 min = 8:05 cutoff; 8:06 = late)
- ✅ Automatic late computation (minutes counted from 8:00, not from grace period end)
- ✅ Overtime tracking with configurable threshold
- ✅ Undertime computation
- ✅ Manual attendance adjustment (HR only)
- ✅ Attendance history & summary reports
- ✅ **Fully adjustable settings** (grace period, work hours, deduction types)

### Sprint 3 — Leave Management
- ✅ Leave types (VL, SL, Emergency, Maternity, Paternity)
- ✅ Leave credit allocation & tracking
- ✅ Leave application with approval workflow
- ✅ Auto-deduction of leave credits
- ✅ Leave balance display

### Sprint 4 — Payroll Integration
- ✅ **BIR TRAIN Law (RA 10963) tax computation** — RR 2-2023 (2023 onwards)
- ✅ **SSS contribution** (2026 table, 4.5% employee share)
- ✅ **PhilHealth contribution** (2.5% employee share, ₱500–₱5,000 cap)
- ✅ **Pag-IBIG contribution** (2%, max ₱100/month)
- ✅ Overtime pay computation (DOLE rates: 1.25x regular, 1.30x rest day, 2.0x holiday)
- ✅ Night differential (10% of hourly rate, 10pm–6am)
- ✅ Late & undertime deductions (auto-computed from daily rate or fixed amount)
- ✅ Payroll computation per cutoff (15th & 30th)
- ✅ Payslip generation (PDF)

### Sprint 5 — Reports, Testing & Optimization
- ✅ Attendance summary reports
- ✅ Leave summary reports
- ✅ Payroll summary reports
- ✅ Audit trail (all user actions logged)
- ✅ Dashboard with real-time stats
- ✅ Upcoming birthdays widget
- ✅ Recent activity feed

### Admin Features
- ✅ **System Settings UI** — adjust all rules without touching code
- ✅ User management
- ✅ Role & permission management
- ✅ Audit log viewer

---

## 🎨 UI/UX Design

- **PECCI Brand Colors** (from official logo):
  - Primary Green: `#2E7D32`
  - Dark Green: `#1B5E20`
  - Accent Orange: `#E8521A`
  - Light Green: `#4CAF50`
- **Bootstrap 5.3** + custom PECCI theme (`pecci-theme.css`)
- **Font Awesome 6.5** icons
- **Chart.js** for dashboard visualizations
- **Responsive design** (mobile-friendly)
- **PECCI SVG logo** rendered inline in sidebar and login page

---

## 🛠️ Technology Stack

- **Framework:** ASP.NET Core MVC (.NET 10.0)
- **ORM:** Entity Framework Core 9.0
- **Database:** SQL Server 2019+ / LocalDB
- **Authentication:** Cookie-based with BCrypt password hashing
- **PDF Generation:** iText7
- **Excel Export:** ClosedXML
- **Frontend:** Bootstrap 5.3, Font Awesome 6.5, Chart.js

---

## 📦 Installation & Setup

### Prerequisites
- Visual Studio 2022 (or VS Code with C# extension)
- .NET 8.0 SDK
- SQL Server 2019+ or SQL Server Express
- SQL Server Management Studio (SSMS)

### Step 1: Clone the Repository
```bash
git clone <repository-url>
cd PECCI_HRIS
```

### Step 2: Configure Database Connection

Open `appsettings.json` and update the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PECCI_HRIS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**Common formats:**
- Local SQL Express: `Server=localhost\\SQLEXPRESS;...`
- Named instance: `Server=YOUR_PC_NAME\\SQLEXPRESS;...`
- SQL Auth: `Server=localhost;Database=PECCI_HRIS_DB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;`

### Step 3: Restore NuGet Packages
```bash
dotnet restore
```

### Step 4: Run Migrations

**Via Package Manager Console (Visual Studio):**
```powershell
Add-Migration InitialCreate
Update-Database
```

**Via CLI:**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This will:
- Create the `PECCI_HRIS_DB` database
- Create all tables
- Seed default data (roles, admin user, departments, leave types, system settings)

### Step 5: (Optional) Add Sample Data

Open SSMS, connect to your server, and run:
```sql
-- File: Database/01_Seed_Data.sql
```

This adds sample employees and leave credits for testing.

### Step 6: Run the Application
```bash
dotnet run
```

Or press **F5** in Visual Studio.

Navigate to: `https://localhost:5001` (or the port shown in the console)

### Step 7: Login

**Default credentials:**
- Username: `admin`
- Password: `Admin@123`

> ⚠️ **Change this password immediately after first login!**

---

## ⚙️ Configuration & Adjustable Settings

All attendance, payroll, and tax rules are **fully adjustable** via the **System Settings** page (Admin → Settings).

### Attendance Rules (Adjustable)
| Setting | Default | Description |
|---|---|---|
| `WorkStartTime` | 08:00 | Official work start time |
| `WorkEndTime` | 17:00 | Official work end time |
| `GracePeriodMinutes` | 5 | Grace period in minutes (08:00 + 5 = 08:05 cutoff) |
| `GracePeriodSeconds` | 0 | Additional grace period in seconds |
| `OvertimeThresholdMinutes` | 30 | Minimum OT minutes before pay is credited |
| `LateDeductionType` | PerMinute | How late deductions are computed |
| `LateDeductionAmountPerMinute` | 0 | Fixed deduction per minute (0 = auto from daily rate) |
| `UndertimeDeductionType` | PerMinute | How undertime deductions are computed |

### Payroll Rules (Adjustable)
| Setting | Default | Description |
|---|---|---|
| `CutoffDay1` | 15 | First payroll cutoff day |
| `CutoffDay2` | 30 | Second payroll cutoff day |
| `OvertimeRateMultiplier` | 1.25 | Regular OT rate (DOLE: 125%) |
| `RestDayOvertimeRateMultiplier` | 1.30 | Rest day OT rate (DOLE: 130%) |
| `RegularHolidayRateMultiplier` | 2.00 | Regular holiday rate (DOLE: 200%) |
| `NightDifferentialRate` | 0.10 | Night diff rate (DOLE: 10% of hourly) |

### Tax & Gov't Contributions (Adjustable)
| Setting | Default | Description |
|---|---|---|
| `TaxTableType` | BIR_TRAIN_LAW_2023 | BIR withholding tax table |
| `SSSEmployeeRate` | 0.045 | SSS employee share (4.5%) |
| `PhilHealthRate` | 0.025 | PhilHealth employee share (2.5%) |
| `PagIbigRate` | 0.02 | Pag-IBIG employee rate (2%) |
| `PagIbigMaxContribution` | 100.00 | Pag-IBIG max monthly contribution |

**All settings can be changed from the UI without restarting the app.**

---

## 📊 BIR TRAIN Law Tax Table (2023)

| Annual Taxable Income | Tax |
|---|---|
| Up to ₱250,000 | **Exempt** |
| ₱250,001 – ₱400,000 | 15% of excess over ₱250K |
| ₱400,001 – ₱800,000 | ₱22,500 + 20% of excess over ₱400K |
| ₱800,001 – ₱2,000,000 | ₱102,500 + 25% of excess over ₱800K |
| ₱2,000,001 – ₱8,000,000 | ₱402,500 + 30% of excess over ₱2M |
| Over ₱8,000,000 | ₱2,202,500 + 35% of excess over ₱8M |

**Source:** BIR Revenue Regulations No. 2-2023 (RA 10963 TRAIN Law)

---

## 🧪 Testing

### Test Accounts (after running seed data)
- **HR Admin:** `admin` / `Admin@123`
- **Sample Employees:** EMP-0001 to EMP-0005 (create user accounts via User Management)

### Test Scenarios
1. **Attendance:**
   - Time in at 08:05:00 → On time
   - Time in at 08:05:01 → Late (1 minute from 08:00)
   - Time out after 17:30 → Overtime credited (if ≥30 min)
2. **Leave:**
   - Apply for leave → Manager approves → HR approves → Credits deducted
3. **Payroll:**
   - Compute payroll for a cutoff → Verify tax, SSS, PhilHealth, Pag-IBIG
   - Generate payslip PDF

---

## 📁 Project Structure

```
PECCI_HRIS/
├── Configuration/          # Strongly-typed settings classes
├── Controllers/            # MVC controllers
├── Data/                   # DbContext & migrations
├── Database/               # SQL scripts & setup guides
├── Models/                 # Entity models
├── Services/               # Business logic (attendance, tax, audit)
├── ViewModels/             # View models for forms & displays
├── Views/                  # Razor views
│   ├── Account/            # Login, logout
│   ├── Dashboard/          # Main dashboard
│   ├── Attendance/         # Time in/out, history, adjust
│   ├── Settings/           # System settings UI
│   └── Shared/             # Layout, partials
├── wwwroot/
│   └── css/
│       └── pecci-theme.css # PECCI brand styling
├── appsettings.json        # Configuration
├── Program.cs              # App entry point
└── README.md               # This file
```

---

## 🔒 Security Features

- ✅ Password hashing with BCrypt (cost factor: 11)
- ✅ Role-based authorization
- ✅ Anti-forgery tokens on all forms
- ✅ SQL injection protection (EF Core parameterized queries)
- ✅ Audit trail (all actions logged with IP address)
- ✅ Session timeout (8 hours)
- ✅ HTTPS enforcement (production)

---

## 📝 Credits & License

**Developed by:**
- **Arkin Reinier Aguilar**
- **Maxenne De Guzman**
- **Bernice Elyssa Soriano**
- **Emily Etea**

**For:** PECCI

**Internship Program:** University of Santo Tomas (UST)

**Year:** 2026

---

## 📞 Support

For issues or questions, contact the development team or your HR administrator.

---

**© 2026 PECCI — All rights reserved.**

*This system complies with Philippine labor laws (DOLE), BIR tax regulations (TRAIN Law RA 10963), and government contribution schedules (SSS, PhilHealth, Pag-IBIG) as of 2026.*
