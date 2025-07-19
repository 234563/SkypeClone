using System.Data.SqlClient;
using System.Data;

namespace Infrastructure.Common.Interfaces
{
    public interface IDbHelper
    {
        Task<int> ExecuteNonQueryAsync(string procedureName, SqlParameter[] parameters ,SqlConnection connection = null, SqlTransaction _transaction = null);
        Task<object?> ExecuteScalarAsync(string procedureName, SqlParameter[] parameters);
        Task<DataTable> ExecuteStoredProcedureAsync(string procedureName, SqlParameter[] parameters, SqlConnection connection = null, SqlTransaction _transaction = null);
        
    }

}
