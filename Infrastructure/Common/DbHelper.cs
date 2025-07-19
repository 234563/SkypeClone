using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Infrastructure.Common.Interfaces;

namespace Skype.Infrastructure.Common
{
    public class DbHelper : IDbHelper
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public DbHelper(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        //INSERT, UPDATE, DELETE,   Affected row count (int)
        public async Task<int> ExecuteNonQueryAsync(string procedureName, SqlParameter[] parameters , SqlConnection connection = null , SqlTransaction _transaction = null)
        {
            if(connection == null)
            {
               connection = _connectionFactory.CreateConnection();
            }

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddRange(parameters);
            if(_transaction != null)
                command.Transaction = _transaction;


            // ExecuteScalarAsync returns the first column of the first row
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result); // Convert to int (MessageId)
        }

        // One value (e.g., count, ID, flag)
        public async Task<object?> ExecuteScalarAsync(string procedureName, SqlParameter[] parameters)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddRange(parameters);

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            return await command.ExecuteScalarAsync();
        }

        // Select/query returning multiple rows
        public async Task<DataTable> ExecuteStoredProcedureAsync(string procedureName, SqlParameter[] parameters ,  SqlConnection connection = null, SqlTransaction _transaction = null)
        {
            if (connection == null)
            {
                connection = _connectionFactory.CreateConnection();
            }

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddRange(parameters);

            if (_transaction != null)
                command.Transaction = _transaction;


            using var reader = await command.ExecuteReaderAsync();
            var table = new DataTable();
            table.Load(reader);
            return table;
        }

       

     

        public async Task<DataSet> ExecuteStoredProcedureToDataSetAsync(string procedureName, SqlParameter[] parameters)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddRange(parameters);

            using var adapter = new SqlDataAdapter(command);
            var dataSet = new DataSet();

            await connection.OpenAsync();

            // Fill DataSet with result(s)
            adapter.Fill(dataSet);

            return dataSet;
        }



    }



  
}
