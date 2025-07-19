using Domain.Entities;
using Infrastructure.Common.Interfaces;
using Infrastructure.Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Responses;
using Skype.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositroies
{
    public class UserRoleRepository : DbHelper  , IUserRoleRepository
    {
        private ILogger<UserRoleRepository> logger;
        public UserRoleRepository(ISqlConnectionFactory connectionFactory , ILogger<UserRoleRepository> _logger) : base(connectionFactory)
        {
            logger = _logger;
        }

        public async Task<ApiResponse<UserRole>> GetUserRole(int userId)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@UserId", userId)
                };

                var resultTable = await ExecuteStoredProcedureAsync("UserRole_Select", parameters);

                if (resultTable.Rows.Count == 0)
                {
                    return new ApiResponse<UserRole>
                    {
                        StatusCode = 404,
                        Message = "No roles found for user",
                        Data = null
                    };
                }

                // Map first role (assuming one role per user or taking the first if multiple)
                var row = resultTable.Rows[0];
                var userRole = new UserRole
                {
                    UserId = Convert.ToInt32(row["UserId"]),
                    RoleId = Convert.ToInt32(row["Id"]),
                    Role   = new Role()
                    {
                       Name = row["Name"].ToString()
                    }
                };

                return new ApiResponse<UserRole>
                {
                    StatusCode = 200,
                    Message = "Role retrieved successfully",
                    Data = userRole
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting role for user {UserId}", userId);
                return new ApiResponse<UserRole>
                {
                    StatusCode = 500,
                    Message = $"Error retrieving user role: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
