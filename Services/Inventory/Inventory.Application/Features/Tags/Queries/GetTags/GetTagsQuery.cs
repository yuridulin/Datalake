using Datalake.Contracts.Public.Models.Tags;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Queries.GetTags;

public record GetTagsQuery : IQueryRequest<IEnumerable<TagInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public IEnumerable<int>? SpecificIdentifiers { get; init; }

	public IEnumerable<Guid>? SpecificGuids { get; init; }

	public int? SpecificSourceId { get; init; }
}
