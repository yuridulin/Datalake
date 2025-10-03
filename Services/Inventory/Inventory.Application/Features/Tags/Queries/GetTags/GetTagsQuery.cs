using Datalake.Inventory.Api.Models.Tags;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Queries.GetTags;

public record GetTagsQuery : IQueryRequest<IEnumerable<TagInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public IEnumerable<int>? SpecificIdentifiers { get; init; }

	public IEnumerable<Guid>? SpecificGuids { get; init; }

	public int? SpecificSourceId { get; init; }
}
