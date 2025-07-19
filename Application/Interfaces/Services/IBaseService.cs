using Domain.Entities;
using Shared.Responses;

namespace Application.Interfaces.Services
{
    public interface IBaseService<T>
    {
        Task<T?> GetById(int id);
        Task<ApiResponse<T>> CreateAsync(RefreshToken refreshToken);
        Task<ApiResponse<T>> UpdateAsync(RefreshToken refreshToken);
        Task<ApiResponse<T>> DeleteAsync(int id);
    }

  

}
