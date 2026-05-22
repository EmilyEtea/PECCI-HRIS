# PECCI HRIS — Weekly Progress Report
## PECCI Multipurpose Cooperative — Human Resource Information System

**Project:** PECCI HRIS v1.4.0
**Repository:** https://github.com/EmilyEtea/PECCI-HRIS
**Developed by:** UST Interns — Arkin Reinier Aguilar, Maxenne De Guzman, Bernice Elyssa Soriano, Emily Etea
**Report Period:** April 22, 2026 – May 21, 2026

---

## WEEK 1 — April 22, 2026
### Initial Setup & Core System (Sprint 1–5)

**April 22 — Project Kickoff**

- ✅ Initial commit — PECCI HRIS v1.0.0 project scaffolded
- ✅ Applied PECCI brand theme (green `#2E7D32` + orange `#E8521A`) with custom CSS
- ✅ Resolved `.gitignore` conflict, set up proper .NET gitignore
- ✅ **Complete Sprint 1–5 implementation** — all controllers, views, models, services, and documentation added in a single build with 0 errors

**Modules implemented in Week 1:**

| Module | Details |
|---|---|
| Authentication | Cookie-based login/logout, BCrypt password hashing, role-based access (HR Admin, HR Staff, Manager, Employee) |
| Employee Management | Full CRUD — create, view, edit, deactivate; Department & Position management; auto-generated employee numbers; government ID fields (SSS, PhilHealth, Pag-IBIG, TIN) |
| Attendance | Time In/Out with Philippine-compliant grace period logic; late/overtime/undertime computation; manual adjustment (HR only); attendance history |
| Leave Management | 5 leave types (VL, SL, EL, ML, PL); two-step approval workflow (Manager → HR); leave credit tracking; auto-deduction on approval |
| Payroll | BIR TRAIN Law (RA 10963) tax computation; SSS, PhilHealth, Pag-IBIG contributions; overtime pay (DOLE rates); semi-monthly cutoffs; payslip generation |
| Reports | Employee List, Attendance Summary, Leave Summary, Payroll Summary — all printable |
| System Settings | All attendance and payroll rules adjustable from UI without code changes |
| Audit Trail | All user actions logged with IP address, timestamps, old/new values |
| Dashboard | Real-time stats, attendance donut chart, upcoming birthdays, recent activity feed |

- ✅ Fixed cascade delete cycles (SQL Server error 1785)
- ✅ Fixed decimal column types for payroll fields
- ✅ All 14 database tables created via EF Core migrations
- ✅ PECCI logo integrated in sidebar and login page
- ✅ `_ValidationScriptsPartial` added, CSS variable `--pecci-bg-light` fixed

---

## WEEK 2 — April 23–27, 2026
### Fixes, Scanner, Deductions & Documentation

**April 23**
- ✅ Fixed Employee CRUD — form validation, auto-generate EmployeeNo, input masks
- ✅ Added `KioskSettings` configuration class and registered in DI
- ✅ Implemented **double-scan confirmation** on Scanner Terminal (scan twice within 10s to confirm)
- ✅ Added **name-based barcode lookup** — scanner can match by Employee No, Display Name, or Full Name
- ✅ Added `IsPending` flag to `ScanResult` ViewModel
- ✅ Added `.sln` solution file
- ✅ Removed redundant `Microsoft.AspNetCore.Authentication.Cookies` package
- ✅ Upgraded ClosedXML to fix `System.IO.Packaging` vulnerability
- ✅ Moved `ScanRequest`/`ScanResult` into `PECCI_HRIS.ViewModels` namespace

**April 24**
- ✅ **Initial Draft — Deductions Module** (`EmployeeDeduction` model, `DeductionController`, 3 views)
- ✅ Added `EmployeeDeductions` navigation property to `Employee` model
- ✅ Wired `WithMany(e => e.EmployeeDeductions)` in `ApplicationDbContext`
- ✅ Migration: `AddEmployeeDeductions`

**April 27**
- ✅ Fixed form validation across all employee forms
- ✅ **Deductions integrated into payroll computation** — custom deductions (loans, cash advances) automatically picked up during payroll; status set to "Applied" after compute
- ✅ Fixed undertime deduction bug (was passing `0` instead of actual undertime minutes)
- ✅ Added `OtherDeductions` to `PayslipViewModel` and payslip view
- ✅ Fixed `ReportsController.AttendanceSummary` — replaced anonymous object with strongly-typed `AttendanceSummaryRow`
- ✅ Fixed `Reports/AttendanceSummary.cshtml` — changed from `@model dynamic` to strongly-typed
- ✅ Added `PeriodLabel` to Attendance Summary views
- ✅ Added **PECCI logo as browser tab favicon** (`favicon.png`)
- ✅ Updated Technical Documentation to v1.1.0 with ERD, architecture diagrams, API reference

---

## WEEK 3 — April 28–30, 2026
### PDF Payslips, Excel Exports & Recurring Deductions

**April 28**
- ✅ **PDF Payslip Export** — `PayslipPdfService` using iText7; single and multi-page PDF with PECCI logo watermark
- ✅ **Leave Credit Background Refresh** — `LeaveCreditRefreshJob` (background service); runs 5 seconds after startup; only allocates if no credits exist for the current year (replaces slow blocking call in `Program.cs`)
- ✅ Updated Technical Documentation

**April 30**
- ✅ **Excel Report Export** — `ExcelExportService` using ClosedXML; exports Employee List, Attendance Summary, Leave Summary, Payroll Summary
- ✅ **SweetAlert2** integrated — replaced all browser `confirm()` dialogs with styled SweetAlert2 modals
- ✅ **Recurring Deductions** — `RecurringDeductionSchedule` model, `RecurringDeductionService`, `RecurringDeductionController`, 3 views (Index, Create, Edit); supports installment-based loans with auto-generation per cutoff, pause/resume/cancel
- ✅ Migration: `AddRecurringDeductionSchedules`
- ✅ Updated Technical Documentation

---

## WEEK 4 — May 11–15, 2026
### Holiday Calendar, Night Differential & Email Notifications

**May 11**
- ✅ **Holiday Calendar** — `Holiday` model, `HolidayController`, 3 views; seeded with 2026 Philippine public holidays (Proclamation No. 727); Regular and Special Non-Working holidays
- ✅ Migration: `AddHolidayCalendar`

**May 15**
- ✅ **Night Differential Auto-Computation** — `ComputeTotalNightDifferentialPay` in `AttendanceComputationService`; automatically applied during payroll computation based on DB settings
- ✅ **Email Notifications** — `EmailService` using MailKit/MimeKit; sends notifications on leave submission, leave approval/rejection, and pending HR review
- ✅ **Scanner Terminal improvements** — enhanced UI, better error handling, improved scan feedback
- ✅ Migration: `AddNightDifferentialMinutes`
- ✅ `SystemSettingsService` added — reads settings from DB, overrides `appsettings.json` values at runtime
- ✅ `EmailSettings` configuration class added

---

## WEEK 5 — May 18–21, 2026
### Dashboard Charts, 13th Month Pay, Self-Service Features & Role-Based Access

**May 18**
- ✅ Fixed CSS `--pecci-blue` variable (was undefined, caused broken table header in Settings)
- ✅ Fixed `Program.cs` error handler path (`/Home/Error` → `/Account/Login`, no HomeController exists)
- ✅ Fixed `EmployeeNo` optional validation (was incorrectly set to `[Required]`)
- ✅ Removed dead `BaseController` helper methods (`IsHRAdmin`, `IsHRStaff`, `IsManager`)
- ✅ Added XML doc comments to all services
- ✅ Wired PDF download button on Payslips page
- ✅ Updated README and Technical Documentation to v1.2.0

**May 19**
- ✅ **Dashboard Charts** — 3 new Chart.js charts added:
  - Headcount by Department (bar chart)
  - Monthly Payroll Cost Trend (line chart, last 6 months)
  - Leave Utilization (horizontal bar chart, used vs remaining)
- ✅ **13th Month Pay Module (PD 851)** — compute per calendar year; formula: Total Basic Salary ÷ 12; tax-exempt up to ₱90,000 per TRAIN Law; Excel export; nav link added
- ✅ Added **Deactivate Employee** button and modal to Employee Profile (HR Admin only, with reason selection)
- ✅ Added test accounts for all 6 departments (13 accounts total)
- ✅ Updated seed SQL file
- ✅ Added footer to all missing views; fixed dashboard footer placement

**May 20**
- ✅ **Change Password (self-service)** — all users can change their own password from the user menu; requires current password verification
- ✅ **My Payslips (employee self-service)** — all roles can view and download their own payslips filtered by month/year
- ✅ **Leave Credits redesigned** — categorized view with cards per leave type, progress bars, color-coded balances (green/yellow/red)
- ✅ Fixed leave credits year filter
- ✅ Fixed BCrypt hash for test accounts (was using wrong hash — `password` instead of `Test@1234`)
- ✅ **Role-based dashboard** — Employee role gets personal dashboard (quick actions, own leave utilization); HR/Manager gets full stats + charts
- ✅ **Role-based sidebar** — HR Staff: no Timekeeping; Employee/Manager: My Profile + Leave only; HR Staff gets view access to Audit Trail, Users, Settings
- ✅ HR Staff write actions locked to HR Admin only (Users CRUD, Settings save, Payroll finalize)
- ✅ Updated Technical Documentation to v1.4.0

**May 21**
- ✅ Fixed Employee Create form — EmployeeNo field no longer has conflicting `maxlength="8"` attribute
- ✅ Users CRUD (Create/Edit/Reset/Toggle) locked to HR Admin only (GET + POST)
- ✅ HR Staff Users page shows "View only" instead of action buttons
- ✅ Added leave details view improvements

---

## Summary of All Features Implemented

| Feature | Status | Sprint |
|---|---|---|
| Authentication (Login/Logout/Profile) | ✅ Complete | 1 |
| Employee CRUD + Government IDs | ✅ Complete | 1 |
| Department & Position Management | ✅ Complete | 1 |
| Attendance (Time In/Out, Grace Period, OT, Undertime) | ✅ Complete | 2 |
| Barcode/RFID Scanner Terminal | ✅ Complete | 2 |
| Leave Management (5 types, 2-step approval) | ✅ Complete | 3 |
| Leave Credits (categorized, progress bars) | ✅ Complete | 3 |
| Payroll (BIR TRAIN Law, SSS, PhilHealth, Pag-IBIG) | ✅ Complete | 4 |
| Custom Deductions (loans, cash advances) | ✅ Complete | 4 |
| Recurring Deduction Schedules | ✅ Complete | 4 |
| PDF Payslip Export (iText7, watermark) | ✅ Complete | 4 |
| Excel Report Export (ClosedXML, 4 report types) | ✅ Complete | 5 |
| Holiday Calendar (2026 PH holidays) | ✅ Complete | 5 |
| Night Differential Auto-Computation | ✅ Complete | 5 |
| Email Notifications (MailKit) | ✅ Complete | 5 |
| 13th Month Pay (PD 851, Excel export) | ✅ Complete | 5 |
| Dashboard Charts (3 Chart.js charts) | ✅ Complete | 5 |
| Employee Deactivation (with reason) | ✅ Complete | 5 |
| Change Password (self-service) | ✅ Complete | 5 |
| My Payslips (employee self-service) | ✅ Complete | 5 |
| Role-Based Dashboard Views | ✅ Complete | 5 |
| Role-Based Sidebar Navigation | ✅ Complete | 5 |
| System Settings UI (all rules adjustable) | ✅ Complete | 5 |
| Audit Trail | ✅ Complete | 5 |
| SweetAlert2 Dialogs | ✅ Complete | 5 |
| PECCI Brand Theme + Logo + Favicon | ✅ Complete | 1 |

---

## Technical Stack

| Component | Technology | Version |
|---|---|---|
| Framework | ASP.NET Core MVC | .NET 10.0 |
| ORM | Entity Framework Core | 9.0.4 |
| Database | SQL Server / LocalDB | 2019+ |
| Authentication | Cookie-based + BCrypt | Built-in / 4.0.3 |
| PDF | iText7 | 8.0.4 |
| Excel | ClosedXML | 0.104.2 |
| Email | MailKit / MimeKit | 4.8.0 |
| Frontend | Bootstrap | 5.3.3 |
| Alerts | SweetAlert2 | 11 |
| Charts | Chart.js | 4.4.2 |
| Icons | Font Awesome | 6.5.0 |

---

*Prepared by: UST Interns — Arkin Reinier Aguilar, Maxenne De Guzman, Bernice Elyssa Soriano, Emily Etea*
*© 2026 PECCI Multipurpose Cooperative*
*Repository: https://github.com/EmilyEtea/PECCI-HRIS*
