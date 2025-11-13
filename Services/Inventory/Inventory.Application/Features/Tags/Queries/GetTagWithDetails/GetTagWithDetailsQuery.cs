using Datalake.Contracts.Public.Models.Tags;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Queries.GetTagWithDetails;

public record GetTagWithDetailsQuery : IQueryRequest<TagFullInfo>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required int Id { get; init; }
}
