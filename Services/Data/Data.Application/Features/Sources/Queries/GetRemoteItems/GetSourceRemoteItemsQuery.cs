using Datalake.Contracts.Models.Sources;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Sources.Queries.GetRemoteItems;

public record GetSourceRemoteItemsQuery : IQueryRequest<IEnumerable<SourceItemInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required int SourceId { get; init; }
}
