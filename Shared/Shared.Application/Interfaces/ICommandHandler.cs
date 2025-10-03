namespace Datalake.Shared.Application.Interfaces;

public interface ICommandHandler<TCommand, TResult>
	where TCommand : ICommandRequest
	where TResult : notnull
{
	Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}
