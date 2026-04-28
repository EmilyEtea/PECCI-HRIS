using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Borders;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;
using iText.IO.Image;
using iText.Kernel.Geom;
using PECCI_HRIS.ViewModels;

namespace PECCI_HRIS.Services
{
    public class PayslipPdfService
    {
        private static readonly DeviceRgb GreenColor  = new(0x2d, 0x6a, 0x4f);
        private static readonly DeviceRgb OrangeColor = new(0xe8, 0x6a, 0x1a);
        private static readonly DeviceRgb LightGray   = new(0xf8, 0xf9, 0xfa);
        private static readonly DeviceRgb BorderGray  = new(0xde, 0xe2, 0xe6);
        private static readonly DeviceRgb White       = new(0xff, 0xff, 0xff);

        private readonly IWebHostEnvironment _env;

        public PayslipPdfService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public byte[] GenerateSingle(PayslipViewModel slip)
        {
            var ms = new MemoryStream();
            var writer = new PdfWriter(ms, new WriterProperties());
            writer.SetCloseStream(false);
            using var pdf = new PdfDocument(writer);
            using var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(36, 36, 36, 36);

            var bold    = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            AddPayslipContent(doc, slip, bold, regular);
            AddWatermark(pdf);
            doc.Close();

            return ms.ToArray();
        }

        public byte[] GenerateMultiple(IEnumerable<PayslipViewModel> slips)
        {
            var ms = new MemoryStream();
            var writer = new PdfWriter(ms, new WriterProperties());
            writer.SetCloseStream(false);
            using var pdf = new PdfDocument(writer);
            using var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(36, 36, 36, 36);

            var bold    = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            bool first = true;
            foreach (var slip in slips)
            {
                if (!first) doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                AddPayslipContent(doc, slip, bold, regular);
                first = false;
            }

            AddWatermark(pdf);
            doc.Close();

            return ms.ToArray();
        }

        private void AddWatermark(PdfDocument pdf)
        {
            var logoPath = System.IO.Path.Combine(_env.WebRootPath, "images", "pecci-logo.png");
            if (!File.Exists(logoPath)) return;

            var imageData = ImageDataFactory.Create(logoPath);

            // Preserve aspect ratio — base width on 200pt, derive height
            float wmWidth  = 200f;
            float aspectRatio = imageData.GetHeight() / (float)imageData.GetWidth();
            float wmHeight = wmWidth * aspectRatio;

            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                var page     = pdf.GetPage(i);
                var pageSize = page.GetPageSize();

                float centerX = pageSize.GetWidth()  / 2f;
                float centerY = pageSize.GetHeight() / 2f;

                var canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf);

                var gs = new PdfExtGState().SetFillOpacity(0.06f);
                canvas.SaveState();
                canvas.SetExtGState(gs);

                canvas.ConcatMatrix(
                    AffineTransform.GetRotateInstance(
                        Math.PI / 6,
                        centerX, centerY));

                canvas.AddImageFittedIntoRectangle(
                    imageData,
                    new Rectangle(
                        centerX - wmWidth  / 2f,
                        centerY - wmHeight / 2f,
                        wmWidth,
                        wmHeight),
                    false);

                canvas.RestoreState();
                canvas.Release();
            }
        }

        private void AddPayslipContent(Document doc, PayslipViewModel slip,
                                       PdfFont bold, PdfFont regular)
        {
            // ── Header ────────────────────────────────────────────────────────
            var headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 70, 30 }))
                .UseAllAvailableWidth();

            headerTable.AddCell(new Cell()
                .Add(new Paragraph(slip.CompanyName)
                    .SetFont(bold).SetFontSize(13).SetFontColor(GreenColor))
                .Add(new Paragraph(slip.CompanyAddress)
                    .SetFont(regular).SetFontSize(7).SetFontColor(ColorConstants.GRAY))
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE));

            headerTable.AddCell(new Cell()
                .Add(new Paragraph("PAYSLIP")
                    .SetFont(bold).SetFontSize(16).SetFontColor(White)
                    .SetTextAlignment(TextAlignment.CENTER))
                .SetBackgroundColor(GreenColor)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(8)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE));

            doc.Add(headerTable);
            doc.Add(new LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine(1f))
                .SetMarginTop(4).SetMarginBottom(8));

            // ── Employee Info ─────────────────────────────────────────────────
            var infoTable = new Table(UnitValue.CreatePercentArray(new float[] { 40, 30, 30 }))
                .UseAllAvailableWidth().SetMarginBottom(10);

            AddInfoCell(infoTable, "Employee",     slip.EmployeeName, bold, regular);
            AddInfoCell(infoTable, "Employee No.", slip.EmployeeNo,   bold, regular);
            AddInfoCell(infoTable, "Department",   slip.Department,   bold, regular);
            AddInfoCell(infoTable, "Pay Period",   slip.PayPeriod,    bold, regular);
            AddInfoCell(infoTable, "Days Worked",  $"{slip.DaysWorked} / {slip.DaysWorked + slip.DaysAbsent}", bold, regular);
            AddInfoCell(infoTable, "Status",       slip.Status,       bold, regular);

            doc.Add(infoTable);

            // ── Earnings & Deductions ─────────────────────────────────────────
            var mainTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .UseAllAvailableWidth().SetMarginBottom(10);

            // Build earnings as a flat list of paragraphs in one cell
            var earningsCell = new Cell().SetBorder(Border.NO_BORDER).SetPaddingRight(6);
            earningsCell.Add(new Paragraph("EARNINGS")
                .SetFont(bold).SetFontSize(8).SetFontColor(GreenColor)
                .SetBorderBottom(new SolidBorder(GreenColor, 1.5f)).SetPaddingBottom(3).SetMarginBottom(4));
            earningsCell.Add(AmountRow("Basic Salary", slip.BasicSalary, regular));
            if (slip.OvertimePay > 0)       earningsCell.Add(AmountRow("Overtime Pay",       slip.OvertimePay,       regular));
            if (slip.HolidayPay > 0)        earningsCell.Add(AmountRow("Holiday Pay",        slip.HolidayPay,        regular));
            if (slip.NightDifferential > 0) earningsCell.Add(AmountRow("Night Differential", slip.NightDifferential, regular));
            if (slip.Allowances > 0)        earningsCell.Add(AmountRow("Allowances",         slip.Allowances,        regular));
            earningsCell.Add(new Paragraph($"Gross Pay                    PHP {slip.GrossPay:N2}")
                .SetFont(bold).SetFontSize(9).SetFontColor(GreenColor)
                .SetBorderTop(new SolidBorder(GreenColor, 1f)).SetPaddingTop(3).SetMarginTop(4));

            // Build deductions as a flat list of paragraphs in one cell
            var deductionsCell = new Cell().SetBorder(Border.NO_BORDER).SetPaddingLeft(6);
            deductionsCell.Add(new Paragraph("DEDUCTIONS")
                .SetFont(bold).SetFontSize(8).SetFontColor(OrangeColor)
                .SetBorderBottom(new SolidBorder(OrangeColor, 1.5f)).SetPaddingBottom(3).SetMarginBottom(4));
            if (slip.SSSContribution > 0)        deductionsCell.Add(AmountRow("SSS",              slip.SSSContribution,        regular));
            if (slip.PhilHealthContribution > 0) deductionsCell.Add(AmountRow("PhilHealth",       slip.PhilHealthContribution, regular));
            if (slip.PagIbigContribution > 0)    deductionsCell.Add(AmountRow("Pag-IBIG",         slip.PagIbigContribution,    regular));
            if (slip.WithholdingTax > 0)         deductionsCell.Add(AmountRow("Withholding Tax",  slip.WithholdingTax,         regular));
            if (slip.LateDeductions > 0)         deductionsCell.Add(AmountRow("Late Deductions",  slip.LateDeductions,         regular));
            if (slip.UndertimeDeductions > 0)    deductionsCell.Add(AmountRow("Undertime",        slip.UndertimeDeductions,    regular));
            if (slip.OtherDeductions > 0)        deductionsCell.Add(AmountRow("Other Deductions", slip.OtherDeductions,        regular));
            deductionsCell.Add(new Paragraph($"Total Deductions          PHP {slip.TotalDeductions:N2}")
                .SetFont(bold).SetFontSize(9).SetFontColor(OrangeColor)
                .SetBorderTop(new SolidBorder(OrangeColor, 1f)).SetPaddingTop(3).SetMarginTop(4));

            mainTable.AddCell(earningsCell);
            mainTable.AddCell(deductionsCell);
            doc.Add(mainTable);

            // ── Net Pay banner ────────────────────────────────────────────────
            var netTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .UseAllAvailableWidth().SetMarginBottom(10);

            netTable.AddCell(new Cell()
                .Add(new Paragraph("NET PAY")
                    .SetFont(bold).SetFontSize(9).SetFontColor(White)
                    .SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(2))
                .Add(new Paragraph($"PHP {slip.NetPay:N2}")
                    .SetFont(bold).SetFontSize(20).SetFontColor(White)
                    .SetTextAlignment(TextAlignment.CENTER))
                .SetBackgroundColor(GreenColor)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(12));

            doc.Add(netTable);

            // ── Footer ────────────────────────────────────────────────────────
            doc.Add(new Paragraph($"Generated: {slip.GeneratedAt:MMMM dd, yyyy hh:mm tt}")
                .SetFont(regular).SetFontSize(7).SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.RIGHT));
            doc.Add(new Paragraph("This is a system-generated payslip. No signature required.")
                .SetFont(regular).SetFontSize(7).SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.CENTER));
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static void AddInfoCell(Table table, string label, string value,
                                        PdfFont bold, PdfFont regular)
        {
            table.AddCell(new Cell()
                .Add(new Paragraph(label)
                    .SetFont(regular).SetFontSize(7).SetFontColor(ColorConstants.GRAY).SetMarginBottom(1))
                .Add(new Paragraph(value)
                    .SetFont(bold).SetFontSize(9))
                .SetBackgroundColor(LightGray)
                .SetBorder(new SolidBorder(BorderGray, 0.5f))
                .SetPadding(5));
        }

        private static Paragraph AmountRow(string label, decimal amount, PdfFont regular)
        {
            // Tab stop trick: pad label to fixed width using spaces, right-align amount
            return new Paragraph($"{label,-28} PHP {amount,12:N2}")
                .SetFont(regular).SetFontSize(8)
                .SetMarginTop(1).SetMarginBottom(1);
        }
    }
}
