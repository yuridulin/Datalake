using Datalake.Contracts.Models.Tags;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Queries.GetTags;

public interface IGetTagsHandler : IQueryHandler<GetTagsQuery, List<TagSimpleInfo>> { }

public class GetTagsHandler(ITagsQueriesService tagsQueriesService) : IGetTagsHandler
{
	public async Task<List<TagSimpleInfo>> HandleAsync(GetTagsQuery query, CancellationToken ct = default)
	{
		var data = await tagsQueriesService.GetAsync(
			query.SpecificIdentifiers,
			query.SpecificGuids,
			query.SpecificType,
			query.SpecificSourceId,
			ct);

		foreach (var tag in data)
		{
			var rule = query.User.GetAccessToTag(tag.Id);

			tag.AccessRule = new(rule.Id, rule.Access);

			if (!rule.HasAccess(AccessType.Viewer))
			{
				tag.Name = string.Empty;
				tag.Description = string.Empty;
			}
		}

		return data;
	}
}
