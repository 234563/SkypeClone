using Application.DTOs;
using Application.DTOs.User;
using Application.DTOs.ChatRoom;
using Application.Interfaces;
using Application.Interfaces.FileHandling;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Authentication;
using Infrastructure.Common;
using Infrastructure.Common.Interfaces;
using Infrastructure.Persistence.Interfaces;
using Infrastructure.Persistence.Repositroies;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;
        private readonly PasswordHasher _PasswordHasher;
        private readonly IJwtTokenGenerator jwtTokenGenerator;
        private readonly IRefreshTokenService refreshTokenService;
        private ISqlConnectionFactory sqlConnectionFactory;
        private IFileHelper FileHelper;

        public UserService(IUnitOfWork unitOfWork , ILogger<UserService> logger , PasswordHasher PasswordHasher, IJwtTokenGenerator _jwtTokenGenerator  , 
                           IRefreshTokenService refreshToken , ISqlConnectionFactory _sqlConnectionFactory , IFileHelper fileHelper)
        {
            _unitOfWork = unitOfWork;
            _PasswordHasher = PasswordHasher;
            jwtTokenGenerator = _jwtTokenGenerator;
            _logger = logger;
            refreshTokenService = refreshToken;
            sqlConnectionFactory = _sqlConnectionFactory;
            FileHelper = fileHelper;
        }

        // Register User Method
        public async Task<int> RegisterUserAsync(RegisterUserDto dto)
        {
            using var connection = sqlConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var user = new User
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = _PasswordHasher.HashPassword(dto.Password)
                };

                // Create user within transaction
                var userId = await _unitOfWork.Users.CreateAsync(user, connection, transaction);
                
                if (userId <= 0)
                {
                    transaction.Rollback();
                    return 0;
                }

                // Assign role within transaction
                var roleResult = await _unitOfWork.Users.AssignRoleToUserAsync(userId, 2, connection, transaction);
                if (roleResult.ErrorCode != 0)
                {
                    transaction.Rollback();
                    return 0;
                }

                // Create chatroom for the user
                var chatRoomDto = new Application.DTOs.ChatRoom.CreateChatRoomDTO
                {
                    Name = $"{dto.FullName}'s Chat",
                    IsGroup = false,
                    CreatedAt = DateTime.UtcNow
                };

                var chatRoomId = await _unitOfWork.ChatRooms.CreateChatRoomAsync(chatRoomDto, connection, transaction);
                
                if (chatRoomId <= 0)
                {
                    transaction.Rollback();
                    return 0;
                }

                // Add user as member to the chatroom
                var chatRoomMemberDto = new Application.DTOs.ChatRoom.CreateChatRoomMemberDTO
                {
                    ChatRoomId = chatRoomId,
                    UserId = userId
                };

                var memberResult = await _unitOfWork.ChatRooms.AddChatRoomMember(chatRoomMemberDto, connection, transaction);
                
                if (memberResult <= 0)
                {
                    transaction.Rollback();
                    return 0;
                }

                // Update user's ChatRoomID
                var updateResult = await _unitOfWork.Users.UpdateUserChatRoomIdAsync(userId, chatRoomId, connection, transaction);
                
                if (updateResult <= 0)
                {
                    transaction.Rollback();
                    return 0;
                }

                transaction.Commit();
                return userId;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error registering user.");
                return 0;
            }
        }

        // Login User Method
        public async Task<ApiResponse<LoginResponseDTO>> LoginUserAsync(LoginDTO _user)
        {
            try
            {
                var user = await  _unitOfWork.Users.GetByEmailAsync(_user.Email);
                if (user == null)
                {
                    return ApiResponse<LoginResponseDTO>.ErrorResponse("Invalid Username or password");
                }

                var isValidPassword = _PasswordHasher.VerifyPassword( _user.Password , user.PasswordHash);
                if (!isValidPassword)
                {
                    return ApiResponse<LoginResponseDTO>.ErrorResponse("Invalid Username or password");
                }

                var response =  await  _unitOfWork.UserRole.GetUserRole(user.Id);
                if(response.Data == null)
                {
                    return ApiResponse<LoginResponseDTO>.ErrorResponse("User role not found");
                }
                
                user.UserRole = response.Data;
                //JWT
                var token = jwtTokenGenerator.GenerateToken(user);
                // Refresh Token
                var RFToken  =  await refreshTokenService.GetOrCreateAsync(user.Id);

                LoginResponseDTO loginResponse = new LoginResponseDTO()
                {
                    Id= user.Id,
                    Token = token,
                    RefreshToken = RFToken?.Data?.Refresh_Token,
                    FullName = user.FullName,
                    Email = user.Email,
                    ChatRoomID = user.ChatRoomID
                };

                return ApiResponse<LoginResponseDTO>.SuccessResponse(loginResponse, "Login User" , 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user.");
                return ApiResponse<LoginResponseDTO>.ErrorResponse("Internal server error ");
            }
        }

        // Assign Role to User Method
        public async Task<(int ErrorCode, string Message)> AssignRoleToUserAsync(int userId, int roleId)
        {
            try
            {
                var result = await  _unitOfWork.Users.AssignRoleToUserAsync(userId, roleId);
                if (result.AffectedRows > 0)
                {
                    return (0, "Role assigned to user successfully.");
                }
                return (500, "Internal server error.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user.");
                return (500, "Internal server error.");
            }
        }



        /// <summary>
        ///  Search user except this currentUserId
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public async Task<List<SearchUserResponse>> SearchUsersAsync(string searchTerm, int currentUserId)
        {
            var result = await _unitOfWork.Users.SearchUsersAsync(searchTerm, currentUserId);
           
            if(result == null || result.Count == 0)
            {
                return new List<SearchUserResponse>();
            }

            var response = result.Select( r =>  new SearchUserResponse()
            {
                Id = r.Id,
                FullName = r.FullName,
                Email = r.Email,
            }).ToList();

            return response;

        }

        public async Task<ApiResponse<LoginResponseDTO>> RefreshTokenAsync(string refreshToken)
        {
            // Step 1 get refhesh token from DB 
            var RFtoken = await refreshTokenService.GetByToken(refreshToken);

            // if not expires then return token 
            if (RFtoken == null || RFtoken.ExpiresAt < DateTime.UtcNow)
            {
                return ApiResponse<LoginResponseDTO>.ErrorResponse("Refresh token expires generate new token", 200);
            }

            var users  = await _unitOfWork.Users.SearchUsersByFiltersAsync(RFtoken.UserId, null, null);


            // Generate new token 
            var JWTToken = jwtTokenGenerator.GenerateToken(user: users[0]);

            LoginResponseDTO loginResponse = new LoginResponseDTO()
            {
                Id = RFtoken.UserId,
                Token = JWTToken,
                RefreshToken = RFtoken?.Refresh_Token,
                FullName = users[0].FullName,
                Email = users[0].Email,
            };

            return ApiResponse<LoginResponseDTO>.SuccessResponse(loginResponse, "New Token Generated ", 200);


        }

        public Task<UserInfoDto> GetUserInfoAsync(int userId)
        {
            return _unitOfWork.Users.GetUserInfoAsync(userId);
        }

        public async Task<int> UpdateUserInfo(UserInfoDto User)
        {
           

            try
            {
                if(!string.IsNullOrEmpty(User.ChatAvatar) && FileHelper.IsBase64String(User.ChatAvatar))
                {
                   User.ChatAvatar = FileHelper.SaveBase64ToFile(User.ChatAvatar, User.Id.ToString()+".png", "image");
                }
                return await _unitOfWork.Users.UpdateUserInfo(User);
            }
            catch (Exception ex)
            {
                return  0;

            }

          
        }


        #region not in use for now 
        public async Task<RefreshToken> RefreshJWTToken(string refreshToken)
        {
            // Step 1 get orignal token from DB 
            var token = await refreshTokenService.GetByToken(refreshToken);
            // if not expires then return token 
            if (token != null && token.ExpiresAt > DateTime.UtcNow)
            {
                return token;
            }

            var user = await _unitOfWork.Users.GetByIdAsync(token.UserId);
            if (user == null)
            {
                return null;
            }

            var newAccessToken = jwtTokenGenerator.GenerateRefreshToken();
            RefreshToken rf = new RefreshToken()
            {
                Id = token?.Id ?? 0,
                Refresh_Token = newAccessToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            if (token == null)
            {
                var result = await refreshTokenService.UpdateAsync(rf);
            }
            else
            {
                var result = await refreshTokenService.CreateAsync(rf);
            }

            return new RefreshToken() { Refresh_Token = newAccessToken };
        }

      


        #endregion
    }

}
