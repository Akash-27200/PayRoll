using Microsoft.Data.SqlClient;

namespace Payroll.Infrastructure
{
    public interface IDbConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}
