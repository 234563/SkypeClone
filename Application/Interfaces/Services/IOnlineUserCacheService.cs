using Application.DTOs.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IOnlineUserCacheService
    {
        Task<List<OnlineUserDTO>> GetOnlineUsers();
        Task AddUserToOnlineCache(string userId, string connectionId);
        Task RemoveUserFromOnlineCache(string userId);
    }
} 