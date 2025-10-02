using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Api.Models.Sources;

namespace Datalake.Inventory.Application.Features.Sources.Queries.GetSource;

public interface IGetSourceHandler : IQueryHandler<GetSourceQuery, SourceWithTagsInfo> { }

public class GetSourceHandler(ISourceQueriesService sourceQueriesService) : IGetSourceHandler
{
	public async Task<SourceWithTagsInfo> HandleAsync(GetSourceQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToSource(AccessType.Viewer, query.SourceId);

		var data = await sourceQueriesService.GetWithTagsAsync(query.SourceId, ct)
			?? throw InventoryNotFoundException.NotFoundSource(query.SourceId);

		return data;
	}
}
