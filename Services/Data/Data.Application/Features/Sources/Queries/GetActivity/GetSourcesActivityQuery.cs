using Datalake.Contracts.Models.Sources;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Sources.Queries.GetActivity;

public record class GetSourcesActivityQuery: IQueryRequest<IEnumerable<SourceActivityInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required IEnumerable<int> SourcesId { get; init; }
}
