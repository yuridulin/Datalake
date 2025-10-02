using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;
using Datalake.Inventory.Api.Models.Sources;

namespace Datalake.Inventory.Application.Features.Sources.Queries.GetSource;

public class GetSourceQuery : IQueryRequest<SourceWithTagsInfo>
{
	public required UserAccessEntity User { get; init; }

	public required int SourceId { get; init; }
}
