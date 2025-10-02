using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;
using Datalake.Inventory.Api.Models.Tags;

namespace Datalake.Inventory.Application.Features.Tags.Queries.GetTagWithDetails;

public record GetTagWithDetailsQuery : IQueryRequest<TagFullInfo>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required int Id { get; init; }
}
