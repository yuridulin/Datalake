using Datalake.Contracts.Models.Tags;
using Datalake.Domain.Enums;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Queries.GetTagsWithSettings;

public record GetTagsWithSettingsQuery : IQueryRequest<List<TagWithSettingsInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public IEnumerable<int>? SpecificIdentifiers { get; init; }

	public IEnumerable<Guid>? SpecificGuids { get; init; }

	public TagType? SpecificType { get; init; }

	public int? SpecificSourceId { get; init; }
}
