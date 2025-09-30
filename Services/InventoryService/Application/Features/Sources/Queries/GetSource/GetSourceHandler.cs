using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Domain.Queries;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.InventoryService.Application.Features.Sources.Queries.GetSource;

public interface IGetSourceHandler : IQueryHandler<GetSourceQuery, SourceWithTagsInfo> { }

public class GetSourceHandler(ISourceQueriesService sourceQueriesService) : IGetSourceHandler
{
	public async Task<SourceWithTagsInfo> HandleAsync(GetSourceQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToSource(PublicApi.Enums.AccessType.Viewer, query.SourceId);

		var data = await sourceQueriesService.GetWithTagsAsync(query.SourceId, ct)
			?? throw Errors.NotFoundSource(query.SourceId);

		return data;
	}
}
