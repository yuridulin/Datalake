namespace Datalake.Inventory.Application.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
	Task BeginTransactionAsync(CancellationToken ct = default);

	Task CommitAsync(CancellationToken ct = default);

	Task RollbackAsync(CancellationToken ct = default);

	Task<int> SaveChangesAsync(CancellationToken ct = default);
}
