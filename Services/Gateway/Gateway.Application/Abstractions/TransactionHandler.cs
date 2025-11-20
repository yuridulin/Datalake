using Datalake.Gateway.Application.Interfaces;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Abstractions;

public abstract class TransactionHandler<TCommand, TResult>(IUnitOfWork unitOfWork) : ICommandHandler<TCommand, TResult>
	where TCommand : ICommandRequest
	where TResult : notnull
{
	public abstract Task<TResult> HandleInTransactionAsync(TCommand command, CancellationToken ct = default);

	public async Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default)
	{
		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			var result = await HandleInTransactionAsync(command, ct);
			await unitOfWork.SaveChangesAsync(ct);
			await unitOfWork.CommitAsync(ct);
			return result;
		}
		catch
		{
			await unitOfWork.RollbackAsync(ct);
			throw;
		}
	}
}