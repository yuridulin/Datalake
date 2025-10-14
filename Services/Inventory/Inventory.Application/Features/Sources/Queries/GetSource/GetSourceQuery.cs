using Datalake.Inventory.Api.Models.Sources;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Queries.GetSource;

public class GetSourceQuery : IQueryRequest<SourceWithTagsInfo>
{
	public required UserAccessValue User { get; init; }

	public required int SourceId { get; init; }
}
