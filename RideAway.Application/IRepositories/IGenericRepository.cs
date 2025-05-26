
using System.Linq.Expressions;

namespace RideAway.Application.IRepositories;
public interface IGenericRepository <T> where T : class
{
    Task AddAsync(T entity);
    Task<T?> Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);
    Task<IEnumerable<T?>> GetAllAsync(Expression<Func<T, bool>>? filter = null,
     string? includeProperties = null);
    Task<T?> GetByIdAsync(Guid id);

    Task<IEnumerable<T>> GetByTypeAsync<TType>() where TType : class, T;
    IQueryable<T?> GetList();
    Task RemoveAsync(T entity);
    Task RemoveRange(IEnumerable<T> entities);
}

