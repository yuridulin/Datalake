using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Api.Models.Sources;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.Sources.Queries.GetSource;

public class GetSourceQuery : IQueryRequest<SourceWithTagsInfo>
{
	public required UserAccessEntity User { get; init; }

	public required int SourceId { get; init; }
}
