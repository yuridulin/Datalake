using Datalake.Data.Application.Models.Values;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.DataCollection.Queries.GetValuesCollectStatus;

public record GetValuesCollectStatusQuery : IQueryRequest<IEnumerable<ValueCollectStatus>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required IEnumerable<int> TagsId { get; init; }
}
