using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Models.Tags;

namespace Datalake.InventoryService.Application.Features.Tags.Queries.GetTags;

public record GetTagsQuery : IQueryRequest<IEnumerable<TagInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public IEnumerable<int>? SpecificIdentifiers { get; init; }

	public IEnumerable<Guid>? SpecificGuids { get; init; }

	public int? SpecificSourceId { get; init; }
}
