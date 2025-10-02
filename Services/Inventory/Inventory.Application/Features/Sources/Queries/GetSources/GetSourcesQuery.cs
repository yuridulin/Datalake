using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;
using Datalake.Inventory.Api.Models.Sources;

namespace Datalake.Inventory.Application.Features.Sources.Queries.GetSources;

public class GetSourcesQuery : IQueryRequest<IEnumerable<SourceInfo>>
{
	public required UserAccessEntity User { get; init; }

	public bool WithCustom { get; init; } = false;
}
