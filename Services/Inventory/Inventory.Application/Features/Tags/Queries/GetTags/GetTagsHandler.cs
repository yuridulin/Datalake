using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Models.Tags;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Queries.GetTags;

public interface IGetTagsHandler : IQueryHandler<GetTagsQuery, IEnumerable<TagInfo>> { }

public class GetTagsHandler(ITagsQueriesService tagsQueriesService) : IGetTagsHandler
{
	public async Task<IEnumerable<TagInfo>> HandleAsync(GetTagsQuery query, CancellationToken ct = default)
	{
		var data = await tagsQueriesService.GetAsync(
			identifiers: query.SpecificIdentifiers,
			guids: query.SpecificGuids,
			sourceId: query.SpecificSourceId,
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
