using Datalake.Contracts.Models.Sources;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Queries.GetSourcesWithSettings;

public class GetSourcesWithSettingsQuery : IQueryRequest<List<SourceWithSettingsInfo>>
{
	public required UserAccessValue User { get; init; }

	public bool WithCustom { get; init; } = false;
}
