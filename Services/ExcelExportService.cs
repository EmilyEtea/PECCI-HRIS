using ClosedXML.Excel;
using PECCI_HRIS.Models;
using PECCI_HRIS.ViewModels;

namespace PECCI_HRIS.Services
{
    /// <summary>
    /// Generates Excel (.xlsx) exports for all report types using ClosedXML.
    /// Each method returns a byte array that the controller streams as a file download.
    /// Reports include: Employee List, Attendance Summary, Leave Summary, Payroll Summary.
    /// </summary>
    public class ExcelExportService
    {
        private const string CompanyName = "PECCI Multipurpose Cooperative";

        // ── Employee List ─────────────────────────────────────────────────────────
        public byte[] ExportEmployeeList(IEnumerable<Employee> employees, string? statusFilter, string? deptFilter)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Employee List");

            // Title block
            ws.Cell(1, 1).Value = CompanyName;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = "Employee List Report";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Font.FontSize = 12;
            ws.Cell(3, 1).Value = $"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
            ws.Cell(3, 1).Style.Font.Italic = true;
            if (!string.IsNullOrEmpty(statusFilter))
                ws.Cell(4, 1).Value = $"Filter — Status: {statusFilter}";
            if (!string.IsNullOrEmpty(deptFilter))
                ws.Cell(5, 1).Value = $"Filter — Department: {deptFilter}";

            // Headers
            int headerRow = 7;
            string[] headers = { "Emp. No.", "Last Name", "First Name", "Middle Name",
                                  "Department", "Position", "Employment Status",
                                  "Date Hired", "Date Regularized", "Status",
                                  "Contact Number", "Company Email", "SSS No.",
                                  "PhilHealth No.", "Pag-IBIG No.", "TIN" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(headerRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2d6a4f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Data
            int row = headerRow + 1;
            foreach (var e in employees)
            {
                ws.Cell(row, 1).Value  = e.EmployeeNo;
                ws.Cell(row, 2).Value  = e.LastName;
                ws.Cell(row, 3).Value  = e.FirstName;
                ws.Cell(row, 4).Value  = e.MiddleName ?? "";
                ws.Cell(row, 5).Value  = e.Department?.DepartmentName ?? "";
                ws.Cell(row, 6).Value  = e.Position?.PositionTitle ?? "";
                ws.Cell(row, 7).Value  = e.EmploymentStatus;
                ws.Cell(row, 8).Value  = e.DateHired.ToString("MM/dd/yyyy");
                ws.Cell(row, 9).Value  = e.DateRegularized?.ToString("MM/dd/yyyy") ?? "";
                ws.Cell(row, 10).Value = e.Status;
                ws.Cell(row, 11).Value = e.ContactNumber ?? "";
                ws.Cell(row, 12).Value = e.CompanyEmail ?? "";
                ws.Cell(row, 13).Value = e.SSSNumber ?? "";
                ws.Cell(row, 14).Value = e.PhilHealthNumber ?? "";
                ws.Cell(row, 15).Value = e.PagIbigNumber ?? "";
                ws.Cell(row, 16).Value = e.TINNumber ?? "";

                // Alternate row shading
                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");

                row++;
            }

            // Total row
            ws.Cell(row, 1).Value = $"Total: {employees.Count()} employee(s)";
            ws.Cell(row, 1).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();
            ws.Range(headerRow, 1, row - 1, headers.Length)
              .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(headerRow, 1, row - 1, headers.Length)
              .Style.Border.InsideBorder = XLBorderStyleValues.Hair;

            return ToBytes(wb);
        }

        // ── Attendance Summary ────────────────────────────────────────────────────
        public byte[] ExportAttendanceSummary(IEnumerable<AttendanceSummaryRow> rows,
                                               int month, int year)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Attendance Summary");
            string period = new DateTime(year, month, 1).ToString("MMMM yyyy");

            ws.Cell(1, 1).Value = CompanyName;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = $"Attendance Summary Report — {period}";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(3, 1).Value = $"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
            ws.Cell(3, 1).Style.Font.Italic = true;

            int headerRow = 5;
            string[] headers = { "Emp. No.", "Full Name", "Department",
                                  "Present", "Late", "Absent", "On Leave",
                                  "Late (mins)", "Overtime (mins)", "Total Hours" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(headerRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2d6a4f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int row = headerRow + 1;
            int totPresent = 0, totLate = 0, totAbsent = 0, totLeave = 0;
            double totLateMins = 0, totOTMins = 0, totHours = 0;

            foreach (var r in rows)
            {
                ws.Cell(row, 1).Value  = r.Employee.EmployeeNo;
                ws.Cell(row, 2).Value  = r.Employee.FullName;
                ws.Cell(row, 3).Value  = r.Employee.Department?.DepartmentName ?? "";
                ws.Cell(row, 4).Value  = r.TotalPresent;
                ws.Cell(row, 5).Value  = r.TotalLate;
                ws.Cell(row, 6).Value  = r.TotalAbsent;
                ws.Cell(row, 7).Value  = r.TotalOnLeave;
                ws.Cell(row, 8).Value  = r.TotalLateMinutes;
                ws.Cell(row, 9).Value  = r.TotalOvertimeMinutes;
                ws.Cell(row, 10).Value = Math.Round(r.TotalHoursWorked, 2);

                totPresent  += r.TotalPresent;
                totLate     += r.TotalLate;
                totAbsent   += r.TotalAbsent;
                totLeave    += r.TotalOnLeave;
                totLateMins += r.TotalLateMinutes;
                totOTMins   += r.TotalOvertimeMinutes;
                totHours    += r.TotalHoursWorked;

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");
                row++;
            }

            // Totals row
            ws.Cell(row, 1).Value  = "TOTALS";
            ws.Cell(row, 2).Value  = $"{rows.Count()} employee(s)";
            ws.Cell(row, 4).Value  = totPresent;
            ws.Cell(row, 5).Value  = totLate;
            ws.Cell(row, 6).Value  = totAbsent;
            ws.Cell(row, 7).Value  = totLeave;
            ws.Cell(row, 8).Value  = totLateMins;
            ws.Cell(row, 9).Value  = totOTMins;
            ws.Cell(row, 10).Value = Math.Round(totHours, 2);
            ws.Row(row).Style.Font.Bold = true;
            ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d8f3dc");

            ws.Columns().AdjustToContents();
            ws.Range(headerRow, 1, row, headers.Length)
              .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(headerRow, 1, row, headers.Length)
              .Style.Border.InsideBorder = XLBorderStyleValues.Hair;

            return ToBytes(wb);
        }

        // ── Leave Summary ─────────────────────────────────────────────────────────
        public byte[] ExportLeaveSummary(IEnumerable<LeaveApplication> applications, int year)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Leave Summary");

            ws.Cell(1, 1).Value = CompanyName;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = $"Leave Summary Report — {year}";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(3, 1).Value = $"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
            ws.Cell(3, 1).Style.Font.Italic = true;

            int headerRow = 5;
            string[] headers = { "Emp. No.", "Full Name", "Department",
                                  "Leave Type", "Start Date", "End Date",
                                  "Days", "Reason", "Status" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(headerRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2d6a4f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int row = headerRow + 1;
            foreach (var a in applications.OrderBy(a => a.Employee?.LastName))
            {
                ws.Cell(row, 1).Value = a.Employee?.EmployeeNo ?? "";
                ws.Cell(row, 2).Value = a.Employee?.FullName ?? "";
                ws.Cell(row, 3).Value = a.Employee?.Department?.DepartmentName ?? "";
                ws.Cell(row, 4).Value = a.LeaveType?.LeaveTypeName ?? "";
                ws.Cell(row, 5).Value = a.StartDate.ToString("MM/dd/yyyy");
                ws.Cell(row, 6).Value = a.EndDate.ToString("MM/dd/yyyy");
                ws.Cell(row, 7).Value = (double)a.NumberOfDays;
                ws.Cell(row, 8).Value = a.Reason ?? "";
                ws.Cell(row, 9).Value = a.Status;

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");
                row++;
            }

            ws.Cell(row, 1).Value = $"Total: {applications.Count()} application(s)";
            ws.Cell(row, 1).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();
            ws.Range(headerRow, 1, row - 1, headers.Length)
              .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(headerRow, 1, row - 1, headers.Length)
              .Style.Border.InsideBorder = XLBorderStyleValues.Hair;

            return ToBytes(wb);
        }

        // ── Payroll Summary ───────────────────────────────────────────────────────
        public byte[] ExportPayrollSummary(IEnumerable<PayrollRecord> records, int month, int year)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Payroll Summary");
            string period = new DateTime(year, month, 1).ToString("MMMM yyyy");

            ws.Cell(1, 1).Value = CompanyName;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = $"Payroll Summary Report — {period}";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(3, 1).Value = $"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
            ws.Cell(3, 1).Style.Font.Italic = true;

            int headerRow = 5;
            string[] headers = { "Emp. No.", "Full Name", "Department", "Pay Period",
                                  "Basic Salary", "Overtime", "Gross Pay",
                                  "SSS", "PhilHealth", "Pag-IBIG", "Tax",
                                  "Late Deductions", "Undertime", "Other Deductions",
                                  "Total Deductions", "Net Pay", "Status" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(headerRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2d6a4f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int row = headerRow + 1;
            foreach (var r in records)
            {
                ws.Cell(row, 1).Value  = r.Employee?.EmployeeNo ?? "";
                ws.Cell(row, 2).Value  = r.Employee?.FullName ?? "";
                ws.Cell(row, 3).Value  = r.Employee?.Department?.DepartmentName ?? "";
                ws.Cell(row, 4).Value  = r.PayPeriod;
                ws.Cell(row, 5).Value  = (double)r.BasicSalary;
                ws.Cell(row, 6).Value  = (double)r.OvertimePay;
                ws.Cell(row, 7).Value  = (double)r.StoredGrossPay;
                ws.Cell(row, 8).Value  = (double)r.SSSContribution;
                ws.Cell(row, 9).Value  = (double)r.PhilHealthContribution;
                ws.Cell(row, 10).Value = (double)r.PagIbigContribution;
                ws.Cell(row, 11).Value = (double)r.WithholdingTax;
                ws.Cell(row, 12).Value = (double)r.LateDeductions;
                ws.Cell(row, 13).Value = (double)r.UndertimeDeductions;
                ws.Cell(row, 14).Value = (double)r.OtherDeductions;
                ws.Cell(row, 15).Value = (double)r.StoredTotalDeductions;
                ws.Cell(row, 16).Value = (double)r.StoredNetPay;
                ws.Cell(row, 17).Value = r.Status;

                // Format currency columns
                for (int col = 5; col <= 16; col++)
                    ws.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");
                row++;
            }

            // Totals row
            ws.Cell(row, 1).Value  = "TOTALS";
            ws.Cell(row, 7).Value  = (double)records.Sum(r => r.StoredGrossPay);
            ws.Cell(row, 15).Value = (double)records.Sum(r => r.StoredTotalDeductions);
            ws.Cell(row, 16).Value = (double)records.Sum(r => r.StoredNetPay);
            for (int col = 5; col <= 16; col++)
                ws.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
            ws.Row(row).Style.Font.Bold = true;
            ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d8f3dc");

            ws.Columns().AdjustToContents();
            ws.Range(headerRow, 1, row, headers.Length)
              .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(headerRow, 1, row, headers.Length)
              .Style.Border.InsideBorder = XLBorderStyleValues.Hair;

            return ToBytes(wb);
        }

        private static byte[] ToBytes(XLWorkbook wb)
        {
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ── 13th Month Pay Export ─────────────────────────────────────────────────
        /// <summary>
        /// Exports the 13th month pay computation result to Excel.
        /// Includes employee details, total basic earned, months worked,
        /// 13th month amount, and tax-exempt breakdown per TRAIN Law.
        /// </summary>
        public byte[] ExportThirteenthMonth(PECCI_HRIS.ViewModels.ThirteenthMonthResultViewModel result)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("13th Month Pay");

            ws.Cell(1, 1).Value = CompanyName;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = $"13th Month Pay — {result.Year}";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Font.FontSize = 12;
            ws.Cell(3, 1).Value = $"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
            ws.Cell(3, 1).Style.Font.Italic = true;
            ws.Cell(4, 1).Value = "Basis: PD 851 — Total Basic Salary Earned ÷ 12 | Tax-exempt up to ₱90,000 (TRAIN Law)";
            ws.Cell(4, 1).Style.Font.Italic = true;
            ws.Cell(4, 1).Style.Font.FontColor = XLColor.DarkGray;

            int headerRow = 6;
            string[] headers = {
                "Emp. No.", "Full Name", "Department", "Position",
                "Months Worked", "Total Basic Earned",
                "13th Month Pay", "Tax-Exempt (≤₱90K)", "Taxable Amount"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(headerRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2d6a4f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int row = headerRow + 1;
            foreach (var r in result.Rows)
            {
                ws.Cell(row, 1).Value = r.EmployeeNo;
                ws.Cell(row, 2).Value = r.EmployeeName;
                ws.Cell(row, 3).Value = r.Department;
                ws.Cell(row, 4).Value = r.Position;
                ws.Cell(row, 5).Value = r.MonthsWorked;
                ws.Cell(row, 6).Value = (double)r.TotalBasicEarned;
                ws.Cell(row, 7).Value = (double)r.ThirteenthMonthPay;
                ws.Cell(row, 8).Value = (double)r.TaxExemptAmount;
                ws.Cell(row, 9).Value = (double)r.TaxableAmount;

                // Format currency columns
                for (int col = 6; col <= 9; col++)
                    ws.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");

                row++;
            }

            // Totals row
            ws.Cell(row, 1).Value = "TOTAL";
            ws.Cell(row, 7).Value = (double)result.TotalPayout;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Row(row).Style.Font.Bold = true;
            ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#d8f3dc");

            ws.Columns().AdjustToContents();
            ws.Range(headerRow, 1, row, headers.Length)
              .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(headerRow, 1, row, headers.Length)
              .Style.Border.InsideBorder = XLBorderStyleValues.Hair;

            return ToBytes(wb);
        }
    }
}
