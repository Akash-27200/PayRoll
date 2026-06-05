using Microsoft.Data.SqlClient;
using Payroll.Infrastructure;
using Payroll.Models;
using System.Data;
using Dapper;

namespace Payroll.Repositories
{
    public sealed class PayrollRepository : IPayrollRepository
    {
        private readonly IDbConnectionFactory _factory;

        public PayrollRepository(IDbConnectionFactory factory)
            => _factory = factory;

        // ── Existence check (used by service before calling SP) ──
        public async Task<bool> RunExistsAsync(int month, int year)
        {
            const string sql = @"
            SELECT COUNT(1) FROM PayrollRuns
            WHERE Month = @Month AND Year = @Year;";

            using var conn = _factory.CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(sql, new { Month = month, Year = year });
            return count > 0;
        }

        // ── Execute stored procedure ──────────────────────────
        public async Task<PayrollRun?> ExecutePayrollRunAsync(int month, int year)
        {
            using var conn = _factory.CreateConnection();
            try
            {
                return await conn.QueryFirstOrDefaultAsync<PayrollRun>(
                    "usp_RunPayroll",
                    new { Month = month, Year = year },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex) when (ex.Message.StartsWith("DUPLICATE:"))
            {
                // Race condition safety: translate SP error → null to allow service to return 409
                return null;
            }
        }

        // ── Get full run + details ───────────────────────────
        public async Task<PayrollRun?> GetRunAsync(int month, int year)
        {
            const string runSql = @"
            SELECT  r.RunId, r.Month, r.Year, r.RunDate, r.IsFinalized,
                    COUNT(d.DetailId)        AS EmployeeCount,
                    ISNULL(SUM(d.NetPay), 0) AS TotalNetPay
            FROM  PayrollRuns   r
            LEFT JOIN PayrollDetails d ON d.RunId = r.RunId
            WHERE r.Month = @Month AND r.Year = @Year
            GROUP BY r.RunId, r.Month, r.Year, r.RunDate, r.IsFinalized;";

            const string detailsSql = @"
            SELECT  d.DetailId, d.RunId, d.EmployeeId,
                    e.FullName, e.EmployeeCode, dep.DepartmentName,
                    d.BasicSalary, d.WorkingDays, d.DaysPresent,
                    d.GrossPay, d.PFDeduction, d.ProfessionalTax, d.NetPay
            FROM  PayrollDetails d
            JOIN  Employees   e   ON e.EmployeeId   = d.EmployeeId
            JOIN  Departments dep ON dep.DepartmentId = e.DepartmentId
            WHERE d.RunId = @RunId
            ORDER BY e.FullName;";

            using var conn = _factory.CreateConnection();

            var run = await conn.QueryFirstOrDefaultAsync<PayrollRun>(
                runSql, new { Month = month, Year = year });

            if (run is null) return null;

            run.Details = (await conn.QueryAsync<PayrollDetail>(
                detailsSql, new { run.RunId })).ToList();

            return run;
        }

        // ── Individual payslip ──────────────────────────────
        public async Task<PayrollDetail?> GetSlipAsync(int runId, int employeeId)
        {
            const string sql = @"
            SELECT  d.DetailId, d.RunId, d.EmployeeId,
                    e.FullName, e.EmployeeCode, dep.DepartmentName,
                    d.BasicSalary, d.WorkingDays, d.DaysPresent,
                    d.GrossPay, d.PFDeduction, d.ProfessionalTax, d.NetPay
            FROM  PayrollDetails d
            JOIN  Employees   e   ON e.EmployeeId   = d.EmployeeId
            JOIN  Departments dep ON dep.DepartmentId = e.DepartmentId
            WHERE d.RunId = @RunId AND d.EmployeeId = @EmployeeId;";

            using var conn = _factory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<PayrollDetail>(
                sql, new { RunId = runId, EmployeeId = employeeId });
        }

        // ── Paged details (BONUS) ───────────────────────────
        public async Task<(IEnumerable<PayrollDetail> Items, int TotalCount)>
            GetRunDetailsPagedAsync(int month, int year, int page, int pageSize)
        {
            const string countSql = @"
            SELECT COUNT(*) FROM PayrollDetails d
            JOIN PayrollRuns r ON r.RunId = d.RunId
            WHERE r.Month = @Month AND r.Year = @Year;";

            const string dataSql = @"
            SELECT  d.DetailId, d.RunId, d.EmployeeId,
                    e.FullName, e.EmployeeCode, dep.DepartmentName,
                    d.BasicSalary, d.WorkingDays, d.DaysPresent,
                    d.GrossPay, d.PFDeduction, d.ProfessionalTax, d.NetPay
            FROM  PayrollDetails d
            JOIN  PayrollRuns r   ON r.RunId = d.RunId
            JOIN  Employees   e   ON e.EmployeeId = d.EmployeeId
            JOIN  Departments dep ON dep.DepartmentId = e.DepartmentId
            WHERE r.Month = @Month AND r.Year = @Year
            ORDER BY e.FullName
            OFFSET     @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;";

            using var conn = _factory.CreateConnection();

            var total = await conn.ExecuteScalarAsync<int>(countSql, new { Month = month, Year = year });
            var items = await conn.QueryAsync<PayrollDetail>(dataSql, new
            {
                Month = month,
                Year = year,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            });

            return (items, total);
        }
    }
}
