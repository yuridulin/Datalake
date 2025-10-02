using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Api.Models.Tags;

namespace Datalake.Inventory.Application.Features.Tags.Queries.GetTagWithDetails;

public interface IGetTagWithDetailsHandler : IQueryHandler<GetTagWithDetailsQuery, TagFullInfo> { }

public class GetTagWithDetailsHandler(ITagsQueriesService tagsQueriesService) : IGetTagWithDetailsHandler
{
	public async Task<TagFullInfo> HandleAsync(GetTagWithDetailsQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToTag(AccessType.Viewer, query.Id);

		var data = await tagsQueriesService.GetWithDetailsAsync(query.Id, ct)
			?? throw InventoryNotFoundException.NotFoundTag(query.Id);

		return data;
	}
}
