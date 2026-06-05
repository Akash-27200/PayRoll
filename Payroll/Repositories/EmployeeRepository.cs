using Microsoft.AspNetCore.Connections;
using Payroll.Infrastructure;
using Payroll.Models;
using Dapper;


namespace Payroll.Repositories
{
    public sealed class EmployeeRepository : IEmployeeRepository
    {
        private readonly IDbConnectionFactory _factory;

        public EmployeeRepository(IDbConnectionFactory factory)
            => _factory = factory;

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            const string sql = @"
            SELECT  e.EmployeeId,
                    e.EmployeeCode,
                    e.FullName,
                    e.Email,
                    e.BasicSalary,
                    e.DepartmentId,
                    d.DepartmentName,
                    e.IsActive,
                    e.JoiningDate
            FROM    Employees   e
            JOIN    Departments d ON d.DepartmentId = e.DepartmentId
            WHERE   e.IsActive = 1
            ORDER BY e.FullName;";

            using var conn = _factory.CreateConnection();
            return await conn.QueryAsync<Employee>(sql);
        }

        public async Task<Employee?> GetByIdAsync(int employeeId)
        {
            const string sql = @"
            SELECT  e.EmployeeId,
                    e.EmployeeCode,
                    e.FullName,
                    e.Email,
                    e.BasicSalary,
                    e.DepartmentId,
                    d.DepartmentName,
                    e.IsActive,
                    e.JoiningDate
            FROM    Employees   e
            JOIN    Departments d ON d.DepartmentId = e.DepartmentId
            WHERE   e.EmployeeId = @EmployeeId;";

            using var conn = _factory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Employee>(
                sql, new { EmployeeId = employeeId });
        }
    }
}
