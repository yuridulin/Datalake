using Datalake.Contracts.Models.Sources;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Queries.GetSourceWithSettingsAndTags;

public class GetSourceWithSettingsAndTagsQuery : IQueryRequest<SourceWithSettingsAndTagsInfo>
{
	public required UserAccessValue User { get; init; }

	public required int SourceId { get; init; }
}
