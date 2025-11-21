using Datalake.Contracts.Models.Blocks;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksWithTags;

public record GetBlocksWithTagsQuery(
	UserAccessValue User) : IQueryRequest<IEnumerable<BlockTreeWithTagsInfo>>;
