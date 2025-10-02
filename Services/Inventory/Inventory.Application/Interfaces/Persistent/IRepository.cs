namespace Datalake.Inventory.Application.Interfaces.Persistent;

public interface IRepository<TEntity, TKey>
{
	Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);

	Task AddAsync(TEntity entity, CancellationToken ct = default);

	Task UpdateAsync(TEntity entity, CancellationToken ct = default);

	Task DeleteAsync(TKey id, CancellationToken ct = default);

	Task<bool> ExistsAsync(TKey id, CancellationToken ct = default);

	Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

	Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
}
