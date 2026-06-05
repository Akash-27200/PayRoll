using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PayrollTest
{
    public class PayrollCalculationTests
    {
        // ── Helper — mirrors the stored-procedure logic exactly ──
        private static (decimal Gross, decimal PF, decimal PT, decimal Net)
            Calculate(decimal basicSalary, int workingDays, int daysPresent)
        {
            if (workingDays <= 0)
                throw new ArgumentException("Working days must be > 0", nameof(workingDays));

            var gross = Math.Round((basicSalary / workingDays) * daysPresent, 2);
            var pf = Math.Round(basicSalary * 0.12m, 2);
            var pt = 200.00m;
            var net = gross - pf - pt;
            return (gross, pf, pt, net);
        }

        // ── Test: example from the brief (Ravi Sharma) ───────────
        [Fact]
        public void Calculation_RaviSharma_MatchesBriefExample()
        {
            // Basic: 30,000 | Working days: 26 | Days present: 24
            var (gross, pf, pt, net) = Calculate(30_000m, 26, 24);

            Assert.Equal(27_692.31m, gross);  // 30000/26*24 = 27692.307... → 27692.31
            Assert.Equal(3_600.00m, pf);
            Assert.Equal(200.00m, pt);
            Assert.Equal(23_892.31m, net);
        }

        // ── Test: full attendance ─────────────────────────────────
        [Fact]
        public void Calculation_FullAttendance_GrossEqualsBasic()
        {
            var (gross, pf, pt, net) = Calculate(35_000m, 26, 26);

            Assert.Equal(35_000.00m, gross);
            Assert.Equal(4_200.00m, pf);
            Assert.Equal(200.00m, pt);
            Assert.Equal(30_600.00m, net);
        }

        // ── Test: zero days present (LOP edge case) ───────────────
        [Fact]
        public void Calculation_ZeroDaysPresent_GrossIsZero()
        {
            var (gross, pf, pt, net) = Calculate(40_000m, 26, 0);

            Assert.Equal(0.00m, gross);
            Assert.Equal(4_800.00m, pf);
            Assert.Equal(200.00m, pt);
            Assert.Equal(-5_000.00m, net);   // LOP: negative net is expected, see README
        }

        // ── Theory: parameterised across employees ────────────────
        [Xunit.Theory]
        [InlineData(30_000, 26, 24, 27_692.31, 3_600.00, 200.00, 23_892.31)]
        [InlineData(35_000, 26, 26, 35_000.00, 4_200.00, 200.00, 30_600.00)]
        [InlineData(28_000, 26, 20, 21_538.46, 3_360.00, 200.00, 17_978.46)]
        [InlineData(32_000, 26, 22, 27_076.92, 3_840.00, 200.00, 23_036.92)]
        public void Calculation_SeedEmployees_AllCorrect(
            decimal basic, int workDays, int present,
            decimal expGross, decimal expPF, decimal expPT, decimal expNet)
        {
            var (gross, pf, pt, net) = Calculate(basic, workDays, present);

            Assert.Equal(expGross, gross);
            Assert.Equal(expPF, pf);
            Assert.Equal(expPT, pt);
            Assert.Equal(expNet, net);
        }

        // ── Test: rounding edge case ──────────────────────────────
        [Fact]
        public void Calculation_RoundingToTwoDecimalPlaces()
        {
            // 10000 / 3 * 1 = 3333.333... → should round to 3333.33
            var (gross, _, _, _) = Calculate(10_000m, 3, 1);
            Assert.Equal(3_333.33m, gross);
        }

        // ── Test: invalid working days throws ────────────────────
        [Fact]
        public void Calculation_ZeroWorkingDays_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Calculate(30_000m, 0, 0));
        }
    }
}
