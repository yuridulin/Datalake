using Datalake.Data.Application.Models.Values;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.DataCollection.Queries.GetValuesCollectStatus;

public record GetValuesCollectStatusQuery : IQueryRequest<IEnumerable<ValueCollectStatus>>, IWithUserAccess
{
	public required UserAccessEntity User { get ; init; }

	public required IEnumerable<int> TagsId { get; init; }
}
