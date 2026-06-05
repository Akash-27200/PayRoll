using Payroll.DTO;
using Payroll.Models;
using Payroll.Repositories;

namespace Payroll.Services
{
    public sealed class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _repo;

        public PayrollService(IPayrollRepository repo) => _repo = repo;

        // ── Trigger payroll run ───────────────────────────────
        public async Task<(PayrollRun? Run, string? Error, bool IsDuplicate)>
            RunPayrollAsync(int month, int year)
        {
            // Basic business-rule validation
            if (month < 1 || month > 12)
                return (null, "Month must be between 1 and 12.", false);

            if (year < 2000 || year > 2100)
                return (null, "Year must be between 2000 and 2100.", false);

            // Immutability check: return 409 if already finalized
            if (await _repo.RunExistsAsync(month, year))
                return (null,
                        $"Payroll for {month:D2}/{year} has already been finalized and cannot be re-run.",
                        true);

            var run = await _repo.ExecutePayrollRunAsync(month, year);

            if (run is null)
                return (null,
                        "Payroll run failed. Check that attendance records exist for the selected month.",
                        false);

            return (run, null, false);
        }

        // ── Fetch existing run ────────────────────────────────
        public Task<PayrollRun?> GetPayrollRunAsync(int month, int year)
            => _repo.GetRunAsync(month, year);

        // ── Individual payslip ────────────────────────────────
        public Task<PayrollDetail?> GetPayslipAsync(int runId, int employeeId)
            => _repo.GetSlipAsync(runId, employeeId);

        // ── Paged detail list (BONUS) ─────────────────────────
        public async Task<PagedResult<PayrollDetail>>
            GetPayrollPagedAsync(int month, int year, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (items, total) = await _repo.GetRunDetailsPagedAsync(month, year, page, pageSize);

            return new PagedResult<PayrollDetail>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
