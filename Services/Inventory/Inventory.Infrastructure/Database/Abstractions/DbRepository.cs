using Datalake.Inventory.Application.Interfaces.Persistent;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Abstractions;

public abstract class DbRepository<TEntity, TKey>(InventoryDbContext context) : IRepository<TEntity, TKey>
	where TEntity : class
	where TKey : notnull
{
	protected readonly DbSet<TEntity> _set = context.Set<TEntity>();

	public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
	{
		return await _set.FindAsync(new object[] { id }, ct);
	}

	public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default)
	{
		await _set.AddAsync(entity, ct);
	}

	public virtual Task UpdateAsync(TEntity entity, CancellationToken ct = default)
	{
		_set.Update(entity);
		return Task.CompletedTask;
	}

	public virtual async Task DeleteAsync(TKey id, CancellationToken ct = default)
	{
		var entity = await GetByIdAsync(id, ct);
		if (entity != null)
			_set.Remove(entity);
	}

	public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken ct = default)
	{
		return await _set.FindAsync(new object[] { id }, ct) != null;
	}

	public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
	{
		await _set.AddRangeAsync(entities, ct);
	}

	public virtual Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
	{
		_set.RemoveRange(entities);
		return Task.CompletedTask;
	}
}
