using Datalake.Contracts.Models.Tags;
using Datalake.Data.Application.Interfaces.Storage;
using Datalake.Domain.Enums;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Tags.Queries.GetCollectionStatus;

public interface IGetTagsCollectionStatusHandler : IQueryHandler<GetTagsCollectionStatusQuery, List<TagStatusInfo>> { }

public class GetTagsCollectionStatusHandler(
	ITagsCollectionStatusStore collectionStatusStore) : IGetTagsCollectionStatusHandler
{
	public Task<List<TagStatusInfo>> HandleAsync(GetTagsCollectionStatusQuery query, CancellationToken ct = default)
	{
		List<int> allowedTagsId = [];

		foreach (var id in query.TagsId)
		{
			if (query.User.HasAccessToTag(RequiredAccess, id))
				allowedTagsId.Add(id);
		}

		var data = collectionStatusStore.Get(allowedTagsId);
		return Task.FromResult(data.ToList());
	}

	const AccessType RequiredAccess = AccessType.Viewer;
}
