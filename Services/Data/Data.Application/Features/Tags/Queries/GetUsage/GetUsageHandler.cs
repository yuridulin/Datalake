using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Domain.Enums;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Tags.Queries.GetUsage;

public interface IGetUsageHandler : IQueryHandler<GetUsageQuery, IDictionary<int, IDictionary<string, DateTime>>> { }

public class GetUsageHandler(
	ITagsSettingsStore tagsSettingsStore,
	ITagsUsageStore tagsUsageStore) : IGetUsageHandler
{
	public async Task<IDictionary<int, IDictionary<string, DateTime>>> HandleAsync(GetUsageQuery query, CancellationToken ct = default)
	{
		Dictionary<int, IDictionary<string, DateTime>> usage = [];

		if (query.TagsId != null)
		{
			foreach (var tagId in query.TagsId)
			{
				if (query.User.HasAccessToTag(RequiredAccess, tagId))
					usage[tagId] = tagsUsageStore.GetUsage(tagId) ?? Empty;
			}
		}
		else if (query.TagsGuid != null)
		{
			foreach (var tagGuid in query.TagsGuid)
			{
				var tag = tagsSettingsStore.TryGet(tagGuid)
					?? throw new ApplicationException($"Тег не найден по идентификатору: {tagGuid}");

				if (!usage.ContainsKey(tag.TagId) && query.User.HasAccessToTag(RequiredAccess, tag.TagId))
					usage[tag.TagId] = tagsUsageStore.GetUsage(tag.TagId) ?? Empty;
			}
		}

		return usage;
	}

	const AccessType RequiredAccess = AccessType.Viewer;

	static Dictionary<string, DateTime> Empty { get; } = [];
}
