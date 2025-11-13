using Datalake.Contracts.Public.Models.Sources;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Queries.GetSources;

public class GetSourcesQuery : IQueryRequest<IEnumerable<SourceInfo>>
{
	public required UserAccessValue User { get; init; }

	public bool WithCustom { get; init; } = false;
}
