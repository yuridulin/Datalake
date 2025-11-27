using Datalake.Contracts.Models.Tags;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Tags.Queries.GetCollectionStatus;

public record GetTagsCollectionStatusQuery : IQueryRequest<List<TagStatusInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required IEnumerable<int> TagsId { get; init; }
}
