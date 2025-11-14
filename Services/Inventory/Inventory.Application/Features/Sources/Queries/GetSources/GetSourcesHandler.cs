using Datalake.Contracts.Models.Sources;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Queries.GetSources;

public interface IGetSourcesHandler : IQueryHandler<GetSourcesQuery, IEnumerable<SourceInfo>> { }

public class GetSourcesHandler(
	ISourcesQueriesService sourceQueriesService) : IGetSourcesHandler
{
	public async Task<IEnumerable<SourceInfo>> HandleAsync(GetSourcesQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(AccessType.Viewer);

		var data = await sourceQueriesService.GetAsync(query.WithCustom, ct);

		return data;
	}
}
