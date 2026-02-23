using System.Linq.Expressions;
using MongoDB.Driver;

namespace CertStore.Infrastructure.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public Repository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        var cursor = await _collection.FindAsync(filter);
        return await cursor.SingleOrDefaultAsync();
    }

    public async Task<List<T>> GetAllAsync()
    {
        var cursor = await _collection.FindAsync(Builders<T>.Filter.Empty);
        return await cursor.ToListAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        var cursor = await _collection.FindAsync(predicate);
        return await cursor.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        var result = await _collection.DeleteOneAsync(filter);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    public async Task<long> DeleteManyAsync(Expression<Func<T, bool>> predicate)
    {
        var result = await _collection.DeleteManyAsync(predicate);
        return result.DeletedCount;
    }
}
