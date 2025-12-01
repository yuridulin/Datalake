using Datalake.Contracts.Models.Blocks;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksWithTags;

public record GetBlocksWithTagsQuery : IQueryRequest<List<BlockWithTagsInfo>>
{
	public required UserAccessValue User { get; init; }
}
