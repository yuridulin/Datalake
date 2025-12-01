using Datalake.Contracts.Models.Tags;
using Datalake.Data.Application.Interfaces.Storage;
using Datalake.Domain.Enums;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Tags.Queries.GetUsage;

public interface IGetUsageHandler : IQueryHandler<GetUsageQuery, List<TagUsageInfo>> { }

public class GetUsageHandler(
	ITagsSettingsStore tagsSettingsStore,
	ITagsUsageStore tagsUsageStore) : IGetUsageHandler
{
	public async Task<List<TagUsageInfo>> HandleAsync(GetUsageQuery query, CancellationToken ct = default)
	{
		HashSet<int> identifiers = [];
		if (query.TagsId != null)
		{
			foreach (var tagId in query.TagsId)
			{
				if (query.User.HasAccessToTag(RequiredAccess, tagId))
					identifiers.Add(tagId);
			}
		}
		if (query.TagsGuid != null)
		{
			foreach (var tagGuid in query.TagsGuid)
			{
				var tag = tagsSettingsStore.TryGet(tagGuid)
					?? throw new ApplicationException($"Тег не найден по идентификатору: {tagGuid}");

				if (query.User.HasAccessToTag(RequiredAccess, tag.TagId))
					identifiers.Add(tag.TagId);
			}
		}

		List<TagUsageInfo> usageList = [];
		foreach (var tagId in identifiers)
		{
			var usage = tagsUsageStore.Get(tagId);
			if (usage == null)
				continue;

			foreach (var req in usage)
				usageList.Add(new() { TagId = tagId, Request = req.Key, Date = req.Value });
		}

		return usageList;
	}

	const AccessType RequiredAccess = AccessType.Viewer;
}
