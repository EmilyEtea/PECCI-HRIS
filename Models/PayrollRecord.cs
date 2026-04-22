using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PECCI_HRIS.Models
{
    public class PayrollRecord
    {
        [Key]
        public int PayrollID { get; set; }

        public int EmployeeID { get; set; }

        [Required, MaxLength(20)]
        public string PayPeriod { get; set; } = string.Empty; // e.g., "2026-01-1-15"

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        // Earnings
        public decimal BasicSalary { get; set; }

        public decimal OvertimePay { get; set; } = 0;

        public decimal HolidayPay { get; set; } = 0;

        public decimal NightDifferential { get; set; } = 0;

        public decimal Allowances { get; set; } = 0;

        public decimal OtherEarnings { get; set; } = 0;

        // Deductions
        public decimal SSSContribution { get; set; } = 0;

        public decimal PhilHealthContribution { get; set; } = 0;

        public decimal PagIbigContribution { get; set; } = 0;

        public decimal WithholdingTax { get; set; } = 0;

        public decimal LeaveDeductions { get; set; } = 0;

        public decimal LateDeductions { get; set; } = 0;

        public decimal UndertimeDeductions { get; set; } = 0;

        public decimal OtherDeductions { get; set; } = 0;

        // Computed
        [NotMapped]
        public decimal GrossPay => BasicSalary + OvertimePay + HolidayPay + NightDifferential + Allowances + OtherEarnings;

        [NotMapped]
        public decimal TotalDeductions => SSSContribution + PhilHealthContribution + PagIbigContribution +
                                          WithholdingTax + LeaveDeductions + LateDeductions + UndertimeDeductions + OtherDeductions;

        [NotMapped]
        public decimal NetPay => GrossPay - TotalDeductions;

        public decimal StoredGrossPay { get; set; }
        public decimal StoredTotalDeductions { get; set; }
        public decimal StoredNetPay { get; set; }

        // Attendance summary
        public int WorkingDays { get; set; }
        public int DaysWorked { get; set; }
        public int DaysAbsent { get; set; }
        public double TotalOvertimeHours { get; set; }
        public double TotalLateMinutes { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Finalized, Released

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        public DateTime? FinalizedAt { get; set; }

        [ForeignKey("EmployeeID")]
        public virtual Employee? Employee { get; set; }
    }
}
