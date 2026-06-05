using Payroll.Models;
using Payroll.Repositories;

namespace Payroll.Services
{
    public sealed class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeService(IEmployeeRepository repo) => _repo = repo;

        public Task<IEnumerable<Employee>> GetAllEmployeesAsync()
            => _repo.GetAllAsync();

        public Task<Employee?> GetByIdAsync(int employeeId)
            => _repo.GetByIdAsync(employeeId);
    }
}
