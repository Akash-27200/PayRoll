using Payroll.Models;

namespace Payroll.Repositories
{
    public interface IPayrollRepository
    {
        Task<bool> RunExistsAsync(int month, int year);
        Task<PayrollRun?> ExecutePayrollRunAsync(int month, int year);
        Task<PayrollRun?> GetRunAsync(int month, int year);
        Task<PayrollDetail?> GetSlipAsync(int runId, int employeeId);

        Task<(IEnumerable<PayrollDetail> Items, int TotalCount)>
        GetRunDetailsPagedAsync(int month, int year, int page, int pageSize);
    }
}
