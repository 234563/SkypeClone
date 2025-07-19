using System.Data.SqlClient;

namespace Infrastructure.Common.Interfaces
{
    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection();
    }

}
