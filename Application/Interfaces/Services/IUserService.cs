using Application.DTOs;
using Application.DTOs.User;
using Domain.Entities;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IUserService  
    {
        Task<int> RegisterUserAsync(RegisterUserDto user);
        Task<ApiResponse<LoginResponseDTO>> LoginUserAsync(LoginDTO _user);
        Task<(int ErrorCode, string Message)> AssignRoleToUserAsync(int userId, int roleId);
        Task<ApiResponse<LoginResponseDTO>> RefreshTokenAsync(string refreshToken);   /// Get new Jwt TOken 

        //Search User
        Task<List<SearchUserResponse>> SearchUsersAsync(string searchTerm, int currentUserId);

        Task<int> UpdateUserInfo(UserInfoDto User);
        Task<UserInfoDto> GetUserInfoAsync(int userId);
    }
}
