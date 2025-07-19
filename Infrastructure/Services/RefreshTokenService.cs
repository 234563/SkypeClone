using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Authentication;
using Infrastructure.Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IUnitOfWork unitOfwork;
        private readonly ILogger<RefreshTokenService> _logger;
        private readonly IJwtTokenGenerator tokenGenerator;

        public RefreshTokenService(IUnitOfWork _unitOfwork, ILogger<RefreshTokenService> logger , IJwtTokenGenerator _tokenGenerator)
        {
            unitOfwork = _unitOfwork;
            _logger = logger;
            tokenGenerator = _tokenGenerator;
        }

        public async Task<RefreshToken?> GetByUserIdAsync(int userId)
        {
            try
            {
                var result = await unitOfwork.RefreshTokens.GetByUserIdAsync(userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching refresh token for userId {UserId}", userId);
                return null;
            }
        }

        

        public async Task<RefreshToken?> GetById(int id)
        {
            try
            {
                var result = await unitOfwork.RefreshTokens.GetByIdAsync(id);
                return result;
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "Error fetching refresh token for id {Id}", id);
                return null;
            }
        }

        public async Task<ApiResponse<RefreshToken>> CreateAsync(RefreshToken refreshToken)
        {
            try
            {
                var result = await unitOfwork.RefreshTokens.CreateAsync(refreshToken);
                if (result <= 0)
                {
                    return ApiResponse<RefreshToken>.SuccessResponse(new RefreshToken() { Id = result }, "Refresh token created successfully",200);
                }
                else
                {
                    return ApiResponse<RefreshToken>.ErrorResponse("Refresh token creation failed", 0);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error creating refresh token");
                return ApiResponse<RefreshToken>.ErrorResponse("Refresh token creation failed", 0);
            }
        }


        public async Task<ApiResponse<RefreshToken>> UpdateAsync(RefreshToken refreshToken)
        {
            try
            {
                var result =  await unitOfwork.RefreshTokens.UpdateAsync(refreshToken);
                if (result > 0)
                {
                    return ApiResponse<RefreshToken>.SuccessResponse(refreshToken, "Refresh token updated successfully", 200);
                }
                else
                {
                    return ApiResponse<RefreshToken>.ErrorResponse("Refresh token updation failed", 0);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refresh token");
                return ApiResponse<RefreshToken>.ErrorResponse("Refresh token updation failed ", 0);
            }

        }

        public async Task<ApiResponse<RefreshToken>> DeleteAsync(int id)
        {
            try
            {
                var result = await unitOfwork.RefreshTokens.DeleteAsync(id);
                if(result > 0)
                {
                    return ApiResponse<RefreshToken>.SuccessResponse(null, "Refresh token deleted successfully", 200);
                }
                else
                {
                    return ApiResponse<RefreshToken>.ErrorResponse("Refresh token deletion failed", 0);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting refresh token");
                return ApiResponse<RefreshToken>.ErrorResponse("Refresh token deletion failed", 0);
            }
        }


        public async Task<ApiResponse<RefreshToken>> GetOrCreateAsync(int userId)
        {
            try
            {
                // Try to get existing token
                var existingToken = await GetByUserIdAsync(userId);

                if (existingToken != null)
                {
                    if (existingToken.ExpiresAt > DateTime.UtcNow)
                    {
                        var newRefreshToken1 = new RefreshToken
                        {
                            Id = existingToken.Id,
                            UserId = userId,
                            Refresh_Token = tokenGenerator.GenerateRefreshToken(),
                            ExpiresAt = DateTime.UtcNow.AddDays(7) // Standard 7-day expiry
                        };

                        var updateResult = await UpdateAsync(newRefreshToken1);
                        if (updateResult.Data != null)
                        {
                            return ApiResponse<RefreshToken>.SuccessResponse(newRefreshToken1, "Refresh token updated successfully", 200);
                        }
                        return ApiResponse<RefreshToken>.ErrorResponse("Failed to update refresh token", 0);
                    }
                    else
                    {
                        return ApiResponse<RefreshToken>.SuccessResponse(existingToken, "Existing refresh token found", 200);
                    }
                }
                
                // Create new token if none exists or is expired
                var newRefreshToken = new RefreshToken
                {
                    UserId = userId,
                    Refresh_Token = tokenGenerator.GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7) // Standard 7-day expiry
                };

                var createResult = await unitOfwork.RefreshTokens.CreateAsync(newRefreshToken);

                if (createResult > 0)
                {
                    newRefreshToken.Id = createResult;
                    return ApiResponse<RefreshToken>.SuccessResponse(newRefreshToken, "New refresh token created", 201);
                }
                return ApiResponse<RefreshToken>.ErrorResponse("Failed to create refresh token", 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrCreateAsync for user {UserId}", userId);
                return ApiResponse<RefreshToken>.ErrorResponse("Error processing refresh token", 0);
            }
        }

        public async Task<RefreshToken?> GetByToken(string token)
        {
            var result = await unitOfwork.RefreshTokens.GetByToken(token);
            if (result == null)
            {
                return null;
            }
            else
            {
                return result;
            }
        }
    }

}
