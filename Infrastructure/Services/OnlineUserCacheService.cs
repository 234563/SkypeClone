using Application.DTOs.User;
using Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services
{
    public class OnlineUserCacheService : IOnlineUserCacheService
    {
        private readonly IMemoryCache _cache;
        private const string ONLINE_USERS_CACHE_KEY = "online_users";

        public OnlineUserCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<List<OnlineUserDTO>> GetOnlineUsers()
        {
            var onlineUsers = _cache.Get<List<OnlineUserDTO>>(ONLINE_USERS_CACHE_KEY);
            return onlineUsers ?? new List<OnlineUserDTO>();
        }

        public async Task AddUserToOnlineCache(string userId, string connectionId)
        {
            var onlineUsers = await GetOnlineUsers();
            var existingUser = onlineUsers.FirstOrDefault(u => u.UserId == userId);
            if (existingUser == null)
            {
                var newUser = new OnlineUserDTO
                {
                    UserId = userId,
                    ConnectionId = connectionId,
                    ConnectedAt = DateTime.UtcNow,
                    Status = "Online"
                };
                onlineUsers.Add(newUser);
            }
            else
            {
                existingUser.ConnectionId = connectionId;
                existingUser.ConnectedAt = DateTime.UtcNow;
                existingUser.Status = "Online";
            }
            _cache.Set(ONLINE_USERS_CACHE_KEY, onlineUsers, TimeSpan.FromHours(24));
        }

        public async Task RemoveUserFromOnlineCache(string userId)
        {
            var onlineUsers = await GetOnlineUsers();
            var userToRemove = onlineUsers.FirstOrDefault(u => u.UserId == userId);
            if (userToRemove != null)
            {
                onlineUsers.Remove(userToRemove);
                _cache.Set(ONLINE_USERS_CACHE_KEY, onlineUsers, TimeSpan.FromHours(24));
            }
        }
    }
} 