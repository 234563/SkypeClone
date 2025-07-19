using Domain.Entities;
using Infrastructure.Common.Interfaces;
using Infrastructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skype.Infrastructure.Common;
using Infrastructure.Common;

namespace Infrastructure.Persistence.Repositroies
{
    public class RefreshTokenRepository : DbHelper , IRefreshTokenRepository
    {
        public RefreshTokenRepository(ISqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public async Task<RefreshToken?> GetByUserIdAsync(int userId)
        {
            var parameters = new[] { new SqlParameter("@UserId", userId) };
            var table = await ExecuteStoredProcedureAsync("GetRefreshTokenByUserId", parameters);

            if (table.Rows.Count == 0)
                return null;

            var row = table.Rows[0];
            return MapToRefreshToken(row);
        }

        public async Task<RefreshToken?> GetByToken(string token)
        {
            var parameters = new[] { new SqlParameter("@Token", token) };
            var table = await ExecuteStoredProcedureAsync("GetRefreshToken", parameters);

            if (table.Rows.Count == 0)
                return null;

            var row = table.Rows[0];
            return MapToRefreshToken(row);
        }

        private RefreshToken MapToRefreshToken(DataRow row)
        {
            return new RefreshToken
            {
                Id = Convert.ToInt32(row["Id"]),
                Refresh_Token = row["Token"].ToString()!,
                ExpiresAt = Convert.ToDateTime(row["ExpiresAt"]),
                UserId = Convert.ToInt32(row["UserId"])
            };
        }

        public async Task<RefreshToken?> GetByIdAsync(int id)
        {
            try
            {
                

                var parameters = new[]
                {
                    new SqlParameter("@Id", id)
                };

                var resultTable = await ExecuteStoredProcedureAsync("GetRefreshTokenByID", parameters);

                if (resultTable.Rows.Count == 0)
                    return null;

                var row = resultTable.Rows[0];

                return new RefreshToken
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Refresh_Token = row.SafeGet<string>("Token"),
                    ExpiresAt = row.SafeGet<DateTime>("ExpiresAt"),
                    UserId = row.SafeGet<int>("UserId"),
                    // Add other properties as needed
                };
            }
            catch (Exception ex)
            {
                
                throw; // Re-throw to let caller handle
            }
        }

        public async Task<int> CreateAsync(RefreshToken entity)
        {
            try
            {
                var parameters = new[]
                {
                       new SqlParameter("@Token", entity.Refresh_Token),
                       new SqlParameter("@ExpiresAt", entity.ExpiresAt),
                       new SqlParameter("@UserId", entity.UserId)
                };

                var result = await ExecuteStoredProcedureAsync("InsertRefreshToken", parameters);

                if (result.Rows.Count > 0)
                {
                    return Convert.ToInt32(result.Rows[0]["NewId"]);
                }

                return -1; // Return -1 if insertion failed
            }
            catch (Exception ex)
            {
                return 0;
                
                //throw; // Re-throw to let caller handle
            }
        }

        public async Task<int> UpdateAsync(RefreshToken entity)
        {
            try
            {
                // Create parameters for the stored procedure
                var parameters = new[]
                {
                   new SqlParameter("@Id", entity.Id),
                   new SqlParameter("@Token", entity.Refresh_Token),
                   new SqlParameter("@ExpiresAt", entity.ExpiresAt)
                };

                // Execute the stored procedure
                var resultTable = await ExecuteStoredProcedureAsync("UpdateRefreshToken", parameters);

                // Check if we got results and parse the affected rows
                if (resultTable.Rows.Count > 0 &&
                    resultTable.Rows[0]["AffectedRows"] != DBNull.Value)
                {
                    int affectedRows = Convert.ToInt32(resultTable.Rows[0]["AffectedRows"]);
                    return affectedRows;
                }

                return 0; // No rows were updated
            }
            catch (Exception ex)
            {
                return 0;
                
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                // Create parameters for the stored procedure
                var parameters = new[]
                {
                   new SqlParameter("@Id", id)
                };

                // Execute the stored procedure
                var resultTable = await ExecuteNonQueryAsync("DeleteRefreshToken", parameters);

                // Check if we got results and parse the affected rows
                if (resultTable > 0)
                {
                    return resultTable;
                }
                return 0; // No rows were deleted
            }
            catch(Exception ex)
            {
                return 0;

            }
        }
    }

}
