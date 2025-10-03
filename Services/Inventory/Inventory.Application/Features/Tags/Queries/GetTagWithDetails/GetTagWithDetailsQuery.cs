using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Api.Models.Tags;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.Tags.Queries.GetTagWithDetails;

public record GetTagWithDetailsQuery : IQueryRequest<TagFullInfo>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required int Id { get; init; }
}
