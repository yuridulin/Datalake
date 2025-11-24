using Datalake.Contracts.Models.Tags;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Queries.GetTagWithSettingsAndBlocks;

public record GetTagWithDetailsQuery : IQueryRequest<TagWithSettingsAndBlocksInfo>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required int Id { get; init; }
}
