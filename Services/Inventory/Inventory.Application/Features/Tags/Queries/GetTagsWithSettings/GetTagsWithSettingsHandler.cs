using Datalake.Contracts.Models.Tags;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Queries.GetTagsWithSettings;

public interface IGetTagsWithSettingsHandler : IQueryHandler<GetTagsWithSettingsQuery, List<TagWithSettingsInfo>> { }

public class GetTagsWithSettingsHandler(ITagsQueriesService tagsQueriesService) : IGetTagsWithSettingsHandler
{
	public async Task<List<TagWithSettingsInfo>> HandleAsync(GetTagsWithSettingsQuery query, CancellationToken ct = default)
	{
		var data = await tagsQueriesService.GetWithSettingsAsync(
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
