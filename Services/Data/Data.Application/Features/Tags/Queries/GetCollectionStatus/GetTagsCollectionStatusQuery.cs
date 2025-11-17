using Datalake.Data.Application.Models.Values;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Tags.Queries.GetTagsCollectionStatus;

public record GetTagsCollectionStatusQuery : IQueryRequest<IEnumerable<TagCollectionStatus>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required IEnumerable<int> TagsId { get; init; }
}
