using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Values;
using Datalake.Domain.Enums;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.DataCollection.Queries.GetValuesCollectStatus;

public interface IGetValuesCollectStatusHandler : IQueryHandler<GetValuesCollectStatusQuery, IEnumerable<ValueCollectStatus>> { }

public class GetValuesCollectStatusHandler(
	IDataCollectionErrorsStore errorsStore) : IGetValuesCollectStatusHandler
{
	public Task<IEnumerable<ValueCollectStatus>> HandleAsync(GetValuesCollectStatusQuery query, CancellationToken ct = default)
	{
		List<int> allowedTagsId = [];
		AccessType requiredAccess = AccessType.Viewer;

		foreach (var id in query.TagsId)
		{
			if (query.User.HasAccessToTag(requiredAccess, id))
				allowedTagsId.Add(id);
		}

		var data = errorsStore.Get(allowedTagsId);
		return Task.FromResult(data);
	}
}
