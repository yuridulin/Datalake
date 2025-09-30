using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.InventoryService.Application.Features.Sources.Queries.GetSources;

public class GetSourcesQuery : IQueryRequest<IEnumerable<SourceInfo>>
{
	public required UserAccessEntity User { get; init; }

	public bool WithCustom { get; init; } = false;
}
