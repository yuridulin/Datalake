using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksWithTags;

public record GetBlocksWithTagsQuery(
	UserAccessEntity User) : IQueryRequest<IEnumerable<BlockWithTagsInfo>>;
