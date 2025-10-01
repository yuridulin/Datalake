using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Queries;
using Datalake.PublicApi.Models.Tags;

namespace Datalake.InventoryService.Application.Features.Tags.Queries.GetTagWithDetails;

public interface IGetTagWithDetailsHandler : IQueryHandler<GetTagWithDetailsQuery, TagFullInfo> { }

public class GetTagWithDetailsHandler(ITagsQueriesService tagsQueriesService) : IGetTagWithDetailsHandler
{
	public async Task<TagFullInfo> HandleAsync(GetTagWithDetailsQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToTag(PublicApi.Enums.AccessType.Viewer, query.Id);

		var data = await tagsQueriesService.GetWithDetailsAsync(query.Id, ct);

		return data;
	}
}
