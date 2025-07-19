using Domain.Entities;
using Shared.Responses;

namespace Infrastructure.Persistence.Interfaces
{
    public interface IUserRoleRepository
    {
        Task<ApiResponse<UserRole>> GetUserRole(int userId);
    }


}
