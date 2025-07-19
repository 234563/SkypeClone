using Application.DTOs;
using Application.DTOs.User;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Infrastructure.Persistence.Interfaces
{
    public interface IUserRepository  : IRepository<User>
    {
        Task<User?> GetByIdAsync(int id);
        Task<int> CreateAsync(User user);
        Task<int> CreateAsync(User user, SqlConnection connection, SqlTransaction transaction);
        Task<int> UpdateAsync(User user);
        Task<int> DeleteAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<(int ErrorCode, string? ErrorMessage, User? User)> LoginAsync(string email);
        Task<(int ErrorCode, string ErrorMessage, int AffectedRows)> AssignRoleToUserAsync(int userId, int roleId);
        Task<(int ErrorCode, string ErrorMessage, int AffectedRows)> AssignRoleToUserAsync(int userId, int roleId, SqlConnection connection, SqlTransaction transaction);
        Task<List<User>> SearchUsersAsync(string searchTerm, int currentUserId);
        Task<List<UserSearchResult>> SearchUsersWithChatStatusAsync(string searchTerm, int currentUserId);
        Task SetOnlineStatusAsync(int userId, bool isOnline);
        Task<List<User>> SearchUsersByFiltersAsync(int? userId, string? fullName, string? email);
        Task<int> UpdateUserInfo(UserInfoDto User);
        Task<int> UpdateUserChatRoomIdAsync(int userId, int chatRoomId, SqlConnection connection, SqlTransaction transaction);
        Task<UserInfoDto> GetUserInfoAsync(int userId);
    }


}
