using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Values;
using Datalake.Domain.Enums;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Tags.Queries.GetTagsCollectionStatus;

public interface IGetTagsCollectionStatusHandler : IQueryHandler<GetTagsCollectionStatusQuery, IEnumerable<TagCollectionStatus>> { }

public class GetTagsCollectionStatusHandler(
	ITagsCollectionStatusStore collectionStatusStore) : IGetTagsCollectionStatusHandler
{
	public Task<IEnumerable<TagCollectionStatus>> HandleAsync(GetTagsCollectionStatusQuery query, CancellationToken ct = default)
	{
		List<int> allowedTagsId = [];

		foreach (var id in query.TagsId)
		{
			if (query.User.HasAccessToTag(RequiredAccess, id))
				allowedTagsId.Add(id);
		}

		var data = collectionStatusStore.Get(allowedTagsId);
		return Task.FromResult(data);
	}

	const AccessType RequiredAccess = AccessType.Viewer;
}
