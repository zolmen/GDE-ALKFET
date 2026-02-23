using System.Linq.Expressions;

namespace CertStore.Infrastructure.Repository;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<long> DeleteManyAsync(Expression<Func<T, bool>> predicate);
}
