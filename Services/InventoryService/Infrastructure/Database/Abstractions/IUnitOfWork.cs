﻿namespace Datalake.InventoryService.Infrastructure.Database.Abstractions;

public interface IUnitOfWork : IAsyncDisposable
{
	Task BeginTransactionAsync(CancellationToken ct = default);
	Task CommitAsync(CancellationToken ct = default);
	Task RollbackAsync(CancellationToken ct = default);
	Task<int> SaveChangesAsync(CancellationToken ct = default);
}


