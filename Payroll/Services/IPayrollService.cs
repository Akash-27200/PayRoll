using Payroll.DTO;
using Payroll.Models;

namespace Payroll.Services
{
    public interface IPayrollService
    {
        Task<(PayrollRun? Run, string? Error, bool IsDuplicate)>
       RunPayrollAsync(int month, int year);

        Task<PayrollRun?> GetPayrollRunAsync(int month, int year);

        Task<PayrollDetail?> GetPayslipAsync(int runId, int employeeId);

        Task<PagedResult<PayrollDetail>>
            GetPayrollPagedAsync(int month, int year, int page, int pageSize);
    }
}
