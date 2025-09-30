namespace Datalake.InventoryService.Application.Interfaces;

public interface IQueryHandler<TQuery, TResult>
	where TQuery : IQueryRequest<TResult>
	where TResult : class
{
	Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}
