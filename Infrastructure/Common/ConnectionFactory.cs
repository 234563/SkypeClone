using System.Data;
using System.Data.SqlClient;
using Infrastructure.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Common
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

    }

}
