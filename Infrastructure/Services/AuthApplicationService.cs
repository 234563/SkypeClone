using Application.DTOs.User;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Responses;
using Infrastructure.Common;
using Application.DTOs;

namespace Infrastructure.Services
{
    public class AuthApplicationService : IAuthApplicationService
    {
        private readonly IUserService _userService;
        private readonly PasswordHasher passwordHasher;
        private readonly ILogger<AuthApplicationService> _logger;
        private readonly IRefreshTokenService refreshTokenService;

        public AuthApplicationService(
            IUserService userService,
            PasswordHasher _passwordHasher,
            ILogger<AuthApplicationService> logger)
        {
            _userService = userService;
            passwordHasher = _passwordHasher;
            _logger = logger;
        }

      
        public  async Task<ApiResponse<UserRole>> AssignRoleToUserAsync(int userId, int roleid)
        {
            var result = await _userService.AssignRoleToUserAsync(userId, roleid);
            if (result.ErrorCode == 0)
            {
                return ApiResponse<UserRole>.SuccessResponse(null, "Role assigned successfully.", 200);
            }
            else
            {
                return ApiResponse<UserRole>.ErrorResponse("Role assigned successfully.", 0);
            }
        }

        public async Task<ApiResponse<LoginResponseDTO>> LoginUserAsync(LoginDTO user)
        {
            var result = await _userService.LoginUserAsync(user);

            return result;
        }

        public async Task<ApiResponse<LoginResponseDTO>> RefreshTokenAsync(string refreshToken)
        {
            var result = await _userService.RefreshTokenAsync(refreshToken);

            return result;
        }

        public async Task<ApiResponse<User>> RegisterUserAsync(RegisterUserDto user)
        {
            var userid = await _userService.RegisterUserAsync(user);

            if (userid > 0)
            {
                try
                {
                    //await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email.");
                }
                return ApiResponse<User>.SuccessResponse(null, "User created successfully.", 200);
            }
            else
            {
                return ApiResponse<User>.ErrorResponse("User Creation failed", 0);
            }
        }


       
    }

}
