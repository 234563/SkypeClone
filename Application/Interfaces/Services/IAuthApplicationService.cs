using Application.DTOs.User;
using Domain.Entities;
using Shared.Responses;

namespace Application.Interfaces.Services
{
    public interface IAuthApplicationService
    {
        Task<ApiResponse<User>> RegisterUserAsync(RegisterUserDto user);
        Task<ApiResponse<LoginResponseDTO>> LoginUserAsync(LoginDTO user);
        Task<ApiResponse<UserRole>> AssignRoleToUserAsync(int userId, int roleid);
        Task<ApiResponse<LoginResponseDTO>> RefreshTokenAsync(string refreshToken);
    }

}
