using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.InventoryService.Application.Features.Sources.Queries.GetSource;

public class GetSourceQuery : IQueryRequest<SourceWithTagsInfo>
{
	public required UserAccessEntity User { get; init; }

	public required int SourceId { get; init; }
}
