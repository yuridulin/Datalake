namespace Datalake.InventoryService.Application.Interfaces;

public interface IQueryHandler<TQuery, TResult>
		where TQuery : IQuery<TResult>
{
	Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}
