using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Shared.DbService.Entities;
using OsuScoreStats.Shared.DbService.Repositories.Interfaces;

namespace OsuScoreStats.Shared.DbService.Repositories;

public class BaseRepository<T, TKey>(ScoreDataContext db) : IRepository<T, TKey>
    where T : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    protected DbSet<T> Set => db.Set<T>();

    /// <summary>
    /// Get all items
    /// </summary>
    /// <returns>An <see cref="IQueryable"/> that can be used to query all items of class <see cref="T"/></returns>
    public IQueryable<T> GetAll() => Set.AsQueryable();
    
    /// <summary>
    /// Get the <see cref="ScoreDataContext"/>
    /// </summary>
    /// <returns>A <see cref="ScoreDataContext"/></returns>
    public DbContext GetDbContext() => db;
    
    /// <summary>
    /// Get an item by its ID
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe</param>
    /// <returns>Object of class <see cref="T"/> or null</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken"/> is canceled</exception>
    public ValueTask<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default) => 
        Set.FindAsync([id], cancellationToken);
    
    /// <summary>
    /// Get items by their IDs
    /// </summary>
    /// <param name="ids">Item IDs</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe</param>
    /// <returns>List containing existing items of class <see cref="T"/></returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken"/> is canceled</exception>
    public Task<List<T>> GetBulkAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default) =>
        Set
            .Where(i => ids.Contains(i.Id)) 
            .ToListAsync(cancellationToken);
    
    /// <summary>
    /// Add item
    /// </summary>
    /// <param name="item">Populated object of class <see cref="T"/></param>
    public void Create(T item) => Set.Add(item);
    
    /// <summary>
    /// Add items
    /// </summary>
    /// <param name="items">IEnumerable containing populated objects of class <see cref="T"/></param>
    public void CreateBulk(IEnumerable<T> items) => Set.AddRange(items);
    
    /// <summary>
    /// Update item data
    /// </summary>
    /// <param name="item">Populated object of class <see cref="T"/></param>
    public void Update(T item) => Set.Update(item);
    
    /// <summary>
    /// Update items data
    /// </summary>
    /// <param name="items">IEnumerable containing populated objects of class <see cref="T"/></param>
    public void UpdateBulk(IEnumerable<T> items) => Set.UpdateRange(items);
    
    /// <summary>
    /// Delete item data
    /// </summary>
    /// <param name="item">Populated object of class <see cref="T"/></param>
    public void Delete(T item) => Set.Remove(item);
    
    /// <summary>
    /// Delete items data
    /// </summary>
    /// <param name="items"><see cref="IEnumerable{T}"/> containing populated objects of class <see cref="T"/></param>
    public void DeleteBulk(IEnumerable<T> items) => Set.RemoveRange(items);
    
    /// <summary>
    /// Save changes made in this context to the DB
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken to observe</param>
    /// <exception cref="OperationCanceledException">If the CancellationToken is canceled</exception>
    /// <returns>Task that represents the asynchronous save operation. Task result contains the number of state entries written to the DB</returns>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}