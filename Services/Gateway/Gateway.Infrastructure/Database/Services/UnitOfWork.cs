using Datalake.Gateway.Application.Interfaces;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore.Storage;

namespace Datalake.Gateway.Infrastructure.Database.Services;

[Scoped]
public class UnitOfWork(GatewayDbContext db) : IUnitOfWork
{
	private IDbContextTransaction? _transaction;

	public async Task BeginTransactionAsync(CancellationToken ct = default)
	{
		if (_transaction != null)
			throw new InvalidOperationException("Transaction already started.");

		_transaction = await db.Database.BeginTransactionAsync(ct);
	}

	public async Task<int> SaveChangesAsync(CancellationToken ct = default)
	{
		return await db.SaveChangesAsync(ct);
	}

	public async Task CommitAsync(CancellationToken ct = default)
	{
		if (_transaction == null)
			throw new InvalidOperationException("No active transaction.");

		await _transaction.CommitAsync(ct);
		await _transaction.DisposeAsync();
		_transaction = null;
	}

	public async Task RollbackAsync(CancellationToken ct = default)
	{
		if (_transaction == null)
			return;

		await _transaction.RollbackAsync(ct);
		await _transaction.DisposeAsync();
		_transaction = null;
	}

	public async ValueTask DisposeAsync()
	{
		if (_transaction != null)
			await _transaction.DisposeAsync();

		GC.SuppressFinalize(this);
	}
}


