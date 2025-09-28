namespace Datalake.InventoryService.Application.Interfaces;

public interface ICommandHandler<TCommand, TResult>
	where TCommand : ICommand
	where TResult : notnull
{
	Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}
