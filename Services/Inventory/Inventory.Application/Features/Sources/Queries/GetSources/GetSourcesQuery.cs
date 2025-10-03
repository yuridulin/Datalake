using Datalake.Inventory.Api.Models.Sources;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Queries.GetSources;

public class GetSourcesQuery : IQueryRequest<IEnumerable<SourceInfo>>
{
	public required UserAccessEntity User { get; init; }

	public bool WithCustom { get; init; } = false;
}
