using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IRepository<T, TKey>
    where T : class
    where TKey : IEquatable<TKey>
{
    IQueryable<T> GetAll();
    DbContext GetDbContext();
    ValueTask<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<List<T>> GetBulkAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);
    void Create(T item);
    void CreateBulk(IEnumerable<T> items);
    void Update(T item);
    void UpdateBulk(IEnumerable<T> items);
    void Delete(T item);
    void DeleteBulk(IEnumerable<T> items);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
