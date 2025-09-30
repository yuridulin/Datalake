using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Domain.Queries;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.InventoryService.Application.Features.Sources.Queries.GetSources;

public interface IGetSourcesHandler : IQueryHandler<GetSourcesQuery, IEnumerable<SourceInfo>> { }

public class GetSourcesHandler(
	ISourceQueriesService sourceQueriesService) : IGetSourcesHandler
{
	public async Task<IEnumerable<SourceInfo>> HandleAsync(GetSourcesQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Viewer);

		var data = await sourceQueriesService.GetAsync(query.WithCustom, ct);

		return data;
	}
}
