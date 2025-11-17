using Datalake.Contracts.Models.Sources;
using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Domain.Enums;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Sources.Queries.GetActivity;

public interface IGetSourcesActivityHandler : IQueryHandler<GetSourcesActivityQuery, IEnumerable<SourceActivityInfo>> { }

public class GetSourcesActivityHandler(
	ISourcesActivityStore sourcesActivityStore) : IGetSourcesActivityHandler
{
	public Task<IEnumerable<SourceActivityInfo>> HandleAsync(
		GetSourcesActivityQuery query,
		CancellationToken ct = default)
	{
		List<SourceActivityInfo> activity = [];

		foreach (var sourceId in query.SourcesId)
		{
			if (!query.User.HasAccessToSource(RequiredAccess, sourceId))
				continue;

			var record = sourcesActivityStore.Get(sourceId);
			activity.Add(record);
		}

		return Task.FromResult<IEnumerable<SourceActivityInfo>>(activity);
	}

	const AccessType RequiredAccess = AccessType.Viewer;
}
