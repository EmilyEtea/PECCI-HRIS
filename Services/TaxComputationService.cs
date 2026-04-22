using Microsoft.Extensions.Options;
using PECCI_HRIS.Configuration;

namespace PECCI_HRIS.Services
{
    /// <summary>
    /// Philippine BIR withholding tax computation.
    /// Based on TRAIN Law (RA 10963) — 2023 onwards tax table.
    /// Table source: BIR Revenue Regulations No. 8-2018 as amended by RR 2-2023.
    /// System developed: 2026.
    /// </summary>
    public class TaxComputationService
    {
        private readonly PayrollSettings _settings;

        public TaxComputationService(IOptions<PayrollSettings> settings)
        {
            _settings = settings.Value;
        }

        /// <summary>
        /// Computes monthly withholding tax based on taxable income.
        /// Taxable income = gross income - mandatory deductions (SSS, PhilHealth, Pag-IBIG).
        /// </summary>
        public decimal ComputeMonthlyWithholdingTax(decimal monthlyTaxableIncome)
        {
            return _settings.TaxTableType switch
            {
                "BIR_TRAIN_LAW_2023" => ComputeTrainLaw2023(monthlyTaxableIncome),
                "BIR_TRAIN_LAW_2018" => ComputeTrainLaw2018(monthlyTaxableIncome),
                _ => ComputeTrainLaw2023(monthlyTaxableIncome)
            };
        }

        /// <summary>
        /// BIR TRAIN Law — effective January 1, 2023 onwards.
        /// Monthly income tax table (RR 2-2023).
        ///
        /// Annual taxable income brackets:
        ///   ≤ 250,000              → 0%
        ///   250,001 – 400,000      → 15% of excess over 250,000
        ///   400,001 – 800,000      → 22,500 + 20% of excess over 400,000
        ///   800,001 – 2,000,000    → 102,500 + 25% of excess over 800,000
        ///   2,000,001 – 8,000,000  → 402,500 + 30% of excess over 2,000,000
        ///   > 8,000,000            → 2,202,500 + 35% of excess over 8,000,000
        /// </summary>
        private static decimal ComputeTrainLaw2023(decimal monthlyTaxableIncome)
        {
            // Convert to annual for bracket lookup, then divide result by 12
            decimal annual = monthlyTaxableIncome * 12m;
            decimal annualTax;

            if (annual <= 250_000m)
            {
                annualTax = 0m;
            }
            else if (annual <= 400_000m)
            {
                annualTax = (annual - 250_000m) * 0.15m;
            }
            else if (annual <= 800_000m)
            {
                annualTax = 22_500m + (annual - 400_000m) * 0.20m;
            }
            else if (annual <= 2_000_000m)
            {
                annualTax = 102_500m + (annual - 800_000m) * 0.25m;
            }
            else if (annual <= 8_000_000m)
            {
                annualTax = 402_500m + (annual - 2_000_000m) * 0.30m;
            }
            else
            {
                annualTax = 2_202_500m + (annual - 8_000_000m) * 0.35m;
            }

            return Math.Round(annualTax / 12m, 2);
        }

        /// <summary>
        /// BIR TRAIN Law — January 1, 2018 to December 31, 2022 (for reference/historical).
        /// </summary>
        private static decimal ComputeTrainLaw2018(decimal monthlyTaxableIncome)
        {
            decimal annual = monthlyTaxableIncome * 12m;
            decimal annualTax;

            if (annual <= 250_000m)
                annualTax = 0m;
            else if (annual <= 400_000m)
                annualTax = (annual - 250_000m) * 0.20m;
            else if (annual <= 800_000m)
                annualTax = 30_000m + (annual - 400_000m) * 0.25m;
            else if (annual <= 2_000_000m)
                annualTax = 130_000m + (annual - 800_000m) * 0.30m;
            else if (annual <= 8_000_000m)
                annualTax = 490_000m + (annual - 2_000_000m) * 0.32m;
            else
                annualTax = 2_410_000m + (annual - 8_000_000m) * 0.35m;

            return Math.Round(annualTax / 12m, 2);
        }

        /// <summary>
        /// Computes SSS contribution based on the 2026 SSS contribution table.
        /// Employee share = 4.5% of monthly salary credit (MSC).
        /// MSC range: ₱4,000 – ₱30,000.
        /// </summary>
        public decimal ComputeSSSContribution(decimal monthlySalary)
        {
            // SSS Monthly Salary Credit brackets (2026 table)
            decimal msc = monthlySalary switch
            {
                < 4_250m => 4_000m,
                < 4_750m => 4_500m,
                < 5_250m => 5_000m,
                < 5_750m => 5_500m,
                < 6_250m => 6_000m,
                < 6_750m => 6_500m,
                < 7_250m => 7_000m,
                < 7_750m => 7_500m,
                < 8_250m => 8_000m,
                < 8_750m => 8_500m,
                < 9_250m => 9_000m,
                < 9_750m => 9_500m,
                < 10_250m => 10_000m,
                < 10_750m => 10_500m,
                < 11_250m => 11_000m,
                < 11_750m => 11_500m,
                < 12_250m => 12_000m,
                < 12_750m => 12_500m,
                < 13_250m => 13_000m,
                < 13_750m => 13_500m,
                < 14_250m => 14_000m,
                < 14_750m => 14_500m,
                < 15_250m => 15_000m,
                < 15_750m => 15_500m,
                < 16_250m => 16_000m,
                < 16_750m => 16_500m,
                < 17_250m => 17_000m,
                < 17_750m => 17_500m,
                < 18_250m => 18_000m,
                < 18_750m => 18_500m,
                < 19_250m => 19_000m,
                < 19_750m => 19_500m,
                < 20_250m => 20_000m,
                < 20_750m => 20_500m,
                < 21_250m => 21_000m,
                < 21_750m => 21_500m,
                < 22_250m => 22_000m,
                < 22_750m => 22_500m,
                < 23_250m => 23_000m,
                < 23_750m => 23_500m,
                < 24_250m => 24_000m,
                < 24_750m => 24_500m,
                < 25_250m => 25_000m,
                < 25_750m => 25_500m,
                < 26_250m => 26_000m,
                < 26_750m => 26_500m,
                < 27_250m => 27_000m,
                < 27_750m => 27_500m,
                < 28_250m => 28_000m,
                < 28_750m => 28_500m,
                < 29_250m => 29_000m,
                < 29_750m => 29_500m,
                _ => 30_000m
            };

            // Employee share = 4.5% of MSC
            return Math.Round(msc * _settings.SSSEmployeeRate, 2);
        }

        /// <summary>
        /// Computes PhilHealth contribution.
        /// 2026: 5% of basic monthly salary, employee pays half (2.5%).
        /// Minimum: ₱500 (based on ₱10,000 floor), Maximum: ₱5,000 (based on ₱100,000 ceiling).
        /// </summary>
        public decimal ComputePhilHealthContribution(decimal monthlySalary)
        {
            decimal premium = monthlySalary * (_settings.PhilHealthRate * 2m); // total premium
            decimal employeeShare = premium / 2m;

            // Floor: ₱500 employee share (₱10,000 salary floor)
            // Ceiling: ₱5,000 employee share (₱100,000 salary ceiling)
            employeeShare = Math.Max(500m, Math.Min(5_000m, employeeShare));
            return Math.Round(employeeShare, 2);
        }

        /// <summary>
        /// Computes Pag-IBIG (HDMF) contribution.
        /// Salary ≤ ₱1,500 → 1%; Salary > ₱1,500 → 2%.
        /// Maximum employee contribution: ₱100/month (configurable).
        /// </summary>
        public decimal ComputePagIbigContribution(decimal monthlySalary)
        {
            decimal rate = monthlySalary <= 1_500m ? 0.01m : _settings.PagIbigRate;
            decimal contribution = monthlySalary * rate;
            return Math.Round(Math.Min(contribution, _settings.PagIbigMaxContribution), 2);
        }

        /// <summary>
        /// Full payroll deductions breakdown for a given monthly salary.
        /// </summary>
        public GovernmentDeductions ComputeGovernmentDeductions(decimal monthlySalary)
        {
            var sss = ComputeSSSContribution(monthlySalary);
            var philHealth = ComputePhilHealthContribution(monthlySalary);
            var pagIbig = ComputePagIbigContribution(monthlySalary);

            // Taxable income = gross - mandatory deductions
            decimal taxableIncome = monthlySalary - sss - philHealth - pagIbig;
            var tax = ComputeMonthlyWithholdingTax(taxableIncome);

            return new GovernmentDeductions
            {
                SSS = sss,
                PhilHealth = philHealth,
                PagIbig = pagIbig,
                WithholdingTax = tax,
                TaxableIncome = taxableIncome,
                TotalDeductions = sss + philHealth + pagIbig + tax
            };
        }

        /// <summary>Returns the effective tax bracket label for display.</summary>
        public string GetTaxBracketLabel(decimal monthlyTaxableIncome)
        {
            decimal annual = monthlyTaxableIncome * 12m;
            return annual switch
            {
                <= 250_000m => "Exempt (₱0 – ₱250,000)",
                <= 400_000m => "15% bracket (₱250,001 – ₱400,000)",
                <= 800_000m => "20% bracket (₱400,001 – ₱800,000)",
                <= 2_000_000m => "25% bracket (₱800,001 – ₱2,000,000)",
                <= 8_000_000m => "30% bracket (₱2,000,001 – ₱8,000,000)",
                _ => "35% bracket (above ₱8,000,000)"
            };
        }
    }

    public class GovernmentDeductions
    {
        public decimal SSS { get; set; }
        public decimal PhilHealth { get; set; }
        public decimal PagIbig { get; set; }
        public decimal WithholdingTax { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal TotalDeductions { get; set; }
    }
}
