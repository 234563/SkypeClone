using Domain.Entities;
using Infrastructure.Common.Interfaces;
using System.Data.SqlClient;
using System.Data;
using Infrastructure.Persistence.Interfaces;
using Skype.Infrastructure.Common;
using Infrastructure.Common;
using Microsoft.Extensions.Logging;
using System.Security;
using Application.DTOs;
using Application.DTOs.User;

namespace Infrastructure.Persistence.Repositroies
{
    public class UserRepository : DbHelper , IUserRepository
    {
        private readonly ILogger<UserRepository> logger;

        public UserRepository(ILogger<UserRepository> _logger , ISqlConnectionFactory sqlConnection ) : base(sqlConnection)
        {
            logger = _logger;
        }

        public  async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                var parameters = new[] { new SqlParameter("@Id", id) };
                var dt = await ExecuteStoredProcedureAsync("GetUserById", parameters);

                if (dt.Rows.Count == 0) return null;

                DataRow row = dt.Rows[0];
                return new User
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Email = row.SafeGet<string>("Email"),
                    PasswordHash = row.SafeGet<string>("PasswordHash"),
                    FullName =  row.SafeGet<string>("FullName"),
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while fetching user by ID: {ID}", id);
                throw;
            }
        }

        public async Task<int> CreateAsync(User user)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@FullName", user.FullName),
                    new SqlParameter("@Email", user.Email),
                    new SqlParameter("@PasswordHash", user.PasswordHash)
                };
                 

                var resultTable = await ExecuteStoredProcedureAsync("RegisterUser", parameters);

                if (resultTable.Rows.Count > 0)
                {
                    int errorCode = Convert.ToInt32(resultTable.Rows[0]["ErrorCode"]);
                    int affectedRows = Convert.ToInt32(resultTable.Rows[0]["AffectedRows"]);
                    int userid = resultTable.Rows[0].SafeGet<int>("UserId");
                    if (errorCode == 0)
                        return userid;

                    logger.LogWarning("Failed to register user. ErrorCode: {ErrorCode}, Message: {ErrorMessage}",
                                       errorCode, resultTable.Rows[0]["ErrorMessage"]);
                }

                return 0; // No rows affected or error occurred
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while creating user: {User}", user);
                return 0;
            }
        }

        public async Task<int> CreateAsync(User user, SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@FullName", user.FullName),
                    new SqlParameter("@Email", user.Email),
                    new SqlParameter("@PasswordHash", user.PasswordHash)
                };
                 
                var resultTable = await ExecuteStoredProcedureAsync("RegisterUser", parameters, connection, transaction);

                if (resultTable.Rows.Count > 0)
                {
                    int errorCode = Convert.ToInt32(resultTable.Rows[0]["ErrorCode"]);
                    int affectedRows = Convert.ToInt32(resultTable.Rows[0]["AffectedRows"]);
                    int userid = resultTable.Rows[0].SafeGet<int>("UserId");
                    if (errorCode == 0)
                        return userid;

                    logger.LogWarning("Failed to register user. ErrorCode: {ErrorCode}, Message: {ErrorMessage}",
                                       errorCode, resultTable.Rows[0]["ErrorMessage"]);
                }

                return 0; // No rows affected or error occurred
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while creating user: {User}", user);
                return 0;
            }
        }

        public  async Task<int> UpdateAsync(User user)
        { 
            try
            {
                 var parameters = new[]
                 {
                 new SqlParameter("@Id", user.Id),
                 new SqlParameter("@FullName", user.FullName),
                 new SqlParameter("@Email", user.Email),
                 new SqlParameter("@PasswordHash", user.PasswordHash)
                 };
                 return await ExecuteNonQueryAsync("UpdateUser", parameters);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while updating user: {User}", user);
                return 0;
            }
        }

        public  async Task<int> DeleteAsync(int id)
        {
            try
            {
                var parameters = new[] { new SqlParameter("@Id", id) };
                return await ExecuteNonQueryAsync("DeleteUser", parameters);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error occurred while deleting user with ID: {ID}", id);
                return 0;
            }
       
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
               var parameters = new[] { new SqlParameter("@Email", email) };
               var dt = await ExecuteStoredProcedureAsync("LoginUser", parameters);
             
               if (dt.Rows.Count == 0) return null;
             
               var row = dt.Rows[0];
               return new User
               {
                   Id = Convert.ToInt32(row["UserId"]),
                   Email = email,
                   PasswordHash = row.SafeGet<string>("PasswordHash"),
                   FullName =  row.SafeGet<string>("FullName"),
                   ChatRoomID = row.SafeGet<int>("ChatRoomID"),
               };

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while fetching user by email: {Email}", email);
                return null;
            }
        }

        public async Task<(int ErrorCode, string? ErrorMessage, User? User)> LoginAsync(string email)
        {
            try
            {
                var parameters = new[]
                {
                      new SqlParameter("@Email", email)
                };

                var table = await ExecuteStoredProcedureAsync("LoginUser", parameters);

                if (table.Rows.Count == 0)
                    return (9999, "Unexpected error: No data returned.", null);

                var row = table.Rows[0];
                var errorCode = Convert.ToInt32(row["ErrorCode"]);
                var errorMessage = row["ErrorMessage"]?.ToString();

                if (errorCode != 0)
                {
                    return (errorCode, errorMessage, null);
                }

                var user = new User
                {
                    Id = Convert.ToInt32(row["UserId"]),
                    Email = email,
                    PasswordHash = row["PasswordHash"]?.ToString() ?? string.Empty
                };

                return (200, "Success", user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Login failed for email: {Email}", email);
                return (9999, "An error occurred during login.", null);
            }
        }

        public async Task<(int ErrorCode, string ErrorMessage, int AffectedRows)> AssignRoleToUserAsync(int userId, int roleId)
        {
            try
            {
                var parameters = new[]
                {
                  new SqlParameter("@UserId", userId),
                  new SqlParameter("@RoleId", roleId)
                };

                var resultTable = await ExecuteStoredProcedureAsync("AssignRoleToUser", parameters);

                if (resultTable.Rows.Count > 0)
                {
                    var row = resultTable.Rows[0];
                    return (
                             Convert.ToInt32(row["ErrorCode"]),
                             row["ErrorMessage"].ToString() ?? "",
                             Convert.ToInt32(row["AffectedRows"])
                           );
                }

                return (-1, "No result returned.", 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", roleId, userId);
                return (-1, "Exception occurred.", 0);
            }
        }

        public async Task<(int ErrorCode, string ErrorMessage, int AffectedRows)> AssignRoleToUserAsync(int userId, int roleId, SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                var parameters = new[]
                {
                  new SqlParameter("@UserId", userId),
                  new SqlParameter("@RoleId", roleId)
                };

                var resultTable = await ExecuteStoredProcedureAsync("AssignRoleToUser", parameters, connection, transaction);

                if (resultTable.Rows.Count > 0)
                {
                    var row = resultTable.Rows[0];
                    return (
                             Convert.ToInt32(row["ErrorCode"]),
                             row["ErrorMessage"].ToString() ?? "",
                             Convert.ToInt32(row["AffectedRows"])
                           );
                }

                return (-1, "No result returned.", 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", roleId, userId);
                return (-1, "Exception occurred.", 0);
            }
        }

        public async Task<int> UpdateUserChatRoomIdAsync(int userId, int chatRoomId, SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@ChatRoomId", chatRoomId)
                };

                var affectedRows = await ExecuteNonQueryAsync("UpdateUserChatRoomId", parameters, connection, transaction);
                return affectedRows;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating ChatRoomId for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<List<User>> SearchUsersAsync(string searchTerm, int currentUserId)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@SearchTerm", searchTerm),
                    new SqlParameter("@CurrentUserId", currentUserId)
                };

                var dt = await ExecuteStoredProcedureAsync("SearchUsers", parameters);
                var users = new List<User>();

                foreach (DataRow row in dt.Rows)
                {
                    users.Add(new User
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        FullName = row.SafeGet<string>("FullName"),
                        Email = row.SafeGet<string>("Email")
                    });
                }

                return users;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while searching users with term: {SearchTerm}", searchTerm);
                return new List<User>();
            }
        }


        public async Task<List<UserSearchResult>> SearchUsersWithChatStatusAsync(string searchTerm, int currentUserId)
        {
            var parameters = new[]
            {
                  new SqlParameter("@SearchTerm", searchTerm),
                  new SqlParameter("@CurrentUserId", currentUserId)
            };

            var dt = await ExecuteStoredProcedureAsync("SearchUsersWithChatStatus", parameters);

            var users = new List<UserSearchResult>();

            foreach (DataRow row in dt.Rows)
            {
                users.Add(new UserSearchResult
                {
                    Id = row.SafeGet<int>("Id"),
                    FullName = row.SafeGet<string>("FullName"),
                    Email = row.SafeGet<string>("Email"),
                    HasChattedBefore = row.SafeGet<bool>("HasChattedBefore"),
                    IsOnline = row.SafeGet<bool>("IsOnline")
                });
            }

            return users;
        }


        public async Task SetOnlineStatusAsync(int userId, bool isOnline)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@IsOnline", isOnline)
            };

            await ExecuteNonQueryAsync("UpdateUserOnlineStatus", parameters);
        }



        public async Task<List<User>> SearchUsersByFiltersAsync(int? userId, string? fullName, string? email)
        {
            try
            {
                var parameters = new[]
                {
                   new SqlParameter("@UserId", userId ?? (object)DBNull.Value),
                   new SqlParameter("@FullName", fullName ?? (object)DBNull.Value),
                   new SqlParameter("@Email", email ?? (object)DBNull.Value)
                };

                var dt = await ExecuteStoredProcedureAsync("SearchUsersByFilters", parameters);

                var users = new List<User>();

                foreach (DataRow row in dt.Rows)
                {
                    users.Add(new User
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        FullName = row.SafeGet<string>("FullName"),
                        Email = row.SafeGet<string>("Email"),
                        IsOnline = row.SafeGet<bool>("IsOnline")
                    });
                }

                return users;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while searching users by filters.");
                return new List<User>();
            }
        }


        public async Task<int>  UpdateUserInfo(UserInfoDto User)
        {
            try
            {
                var parameters = new[]
                {
                   new SqlParameter("@UserId", User.Id ),
                   new SqlParameter("@FullName", User.FullName ?? (object)DBNull.Value),
                   new SqlParameter("@EnableNotification", User.EnableNotification),
                   new SqlParameter("@AppearInSearchResult", User.AppearInSearchResult),
                   new SqlParameter("@ShowOnlineStatus", User.ShowOnlineStatus),
                   new SqlParameter("@DefaultTheme", User.DefaultTheme),
                   new SqlParameter("@ChatAvatar", User.ChatAvatar)
                };

                var affectedrows = await ExecuteNonQueryAsync("UpdateUserInfo", parameters);

                return affectedrows;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while searching users by filters.");
                return 0;
            }
        }

        public async Task<UserInfoDto> GetUserInfoAsync(int userId)
        {
            try
            {
                var parameters = new[] { new SqlParameter("@UserId", userId) };
                var dt = await ExecuteStoredProcedureAsync("GetUserInfo", parameters);

                if (dt.Rows.Count == 0) return null;

                var row = dt.Rows[0];
                return new UserInfoDto
                {
                    Id = row.SafeGet<int>("UserId"),
                    FullName = row.SafeGet<string>("FullName"),
                    Email = row.SafeGet<string>("Email"),
                    IsOnline = row.SafeGet<bool>("IsOnline"),
                    UserRole = row.SafeGet<string>("UserRole"),
                    EnableNotification = row.SafeGet<bool>("EnableNotification"),
                    AppearInSearchResult = row.SafeGet<bool>("AppearInSearchResult"),
                    ShowOnlineStatus = row.SafeGet<bool>("ShowOnlineStatus"),
                    DefaultTheme = row.SafeGet<int>("DefaultTheme"),
                    ChatAvatar = row.SafeGet<string>("ChatAvatar"),
                    ChatRoomID = row.SafeGet<int>("ChatRoomID"),

                };

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading user ");
                return null;
            }

        }

       
    }

}
