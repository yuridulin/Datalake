using Datalake.Contracts.Models.Tags;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Tags.Queries.GetUsage;

public record GetUsageQuery : IQueryRequest<IEnumerable<TagUsageInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public IEnumerable<int>? TagsId { get; init; }

	public IEnumerable<Guid>? TagsGuid { get; init; }
}
