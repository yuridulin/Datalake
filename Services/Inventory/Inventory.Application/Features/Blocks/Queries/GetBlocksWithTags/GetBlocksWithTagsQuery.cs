using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksWithTags;

public record GetBlocksWithTagsQuery(
	UserAccessEntity User) : IQueryRequest<IEnumerable<BlockWithTagsInfo>>;
