using Datalake.Contracts.Models;
using Datalake.Contracts.Models.Sources;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Queries.GetSourcesWithSettings;

public interface IGetSourcesWithSettingsHandler : IQueryHandler<GetSourcesWithSettingsQuery, IEnumerable<SourceWithSettingsInfo>> { }

public class GetSourcesWithSettingsHandler(
	ISourcesQueriesService sourceQueriesService) : IGetSourcesWithSettingsHandler
{
	public async Task<IEnumerable<SourceWithSettingsInfo>> HandleAsync(GetSourcesWithSettingsQuery query, CancellationToken ct = default)
	{
		var sources = await sourceQueriesService.GetAllAsync(query.WithCustom, ct);

		// Защита
		foreach (var source in sources)
		{
			var access = query.User.GetAccessToSource(source.Id);
			source.AccessRule = AccessRuleInfo.FromRule(access);

			if (!access.HasAccess(AccessType.Viewer))
			{
				// Сброс чувствительной информации
				source.Address = null;
				source.Name = string.Empty;
				source.Description = string.Empty;
			}
		}

		return sources;
	}
}
