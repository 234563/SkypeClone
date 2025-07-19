using Domain.Entities;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IRefreshTokenService  : IBaseService<RefreshToken>
    {
        Task<RefreshToken?> GetByUserIdAsync(int userId);
        Task<ApiResponse<RefreshToken>> GetOrCreateAsync(int userId);
        Task<RefreshToken?> GetByToken(string token);
    }

}
