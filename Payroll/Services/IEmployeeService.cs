using Payroll.Models;

namespace Payroll.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetByIdAsync(int employeeId);
    }
}
