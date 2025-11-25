using Datalake.Contracts.Models;
using Datalake.Contracts.Models.Sources;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Queries.GetSourceWithSettingsAndTags;

public interface IGetSourceWithSettingsAndTagsHandler : IQueryHandler<GetSourceWithSettingsAndTagsQuery, SourceWithSettingsAndTagsInfo> { }

public class GetSourceWithSettingsAndTagsHandler(ISourcesQueriesService sourceQueriesService) : IGetSourceWithSettingsAndTagsHandler
{
	public async Task<SourceWithSettingsAndTagsInfo> HandleAsync(GetSourceWithSettingsAndTagsQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToSource(AccessType.Viewer, query.SourceId);

		var source = await sourceQueriesService.GetByIdAsync(query.SourceId, ct)
			?? throw InventoryNotFoundException.NotFoundSource(query.SourceId);

		var tags = source.Type switch
		{
			SourceType.Inopc => await sourceQueriesService.GetSourceTagsAsync(query.SourceId, ct),
			_ => [],
		};

		// Защита тегов
		foreach (var tag in tags)
		{
			var access = query.User.GetAccessToTag(tag.Id);
			tag.AccessRule = AccessRuleInfo.FromRule(access);

			if (!access.HasAccess(AccessType.Viewer))
			{
				// Сброс чувствительных полей
				tag.Id = 0;
				tag.Name = string.Empty;
			}
		}

		return new SourceWithSettingsAndTagsInfo
		{
			Id = source.Id,
			AccessRule = AccessRuleInfo.FromRule(query.User.GetAccessToSource(source.Id)),
			Address = source.Address,
			Description = source.Description,
			IsDisabled = source.IsDisabled,
			Name = source.Name,
			Type = source.Type,
			Tags = tags,
		};
	}
}
