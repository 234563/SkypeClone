namespace Infrastructure.Persistence.Interfaces
{
    public interface IRepository<T>
    {
        Task<T?>  GetByIdAsync(int id);
        Task<int> CreateAsync(T entity);
        Task<int> UpdateAsync(T entity);
        Task<int> DeleteAsync(int id);
    }


}
