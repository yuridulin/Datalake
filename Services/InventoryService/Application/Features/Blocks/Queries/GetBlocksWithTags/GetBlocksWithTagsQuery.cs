using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Features.Blocks.Queries.BlocksWithTags;

public record GetBlocksWithTagsQuery(
	UserAccessEntity User) : IQuery<IEnumerable<BlockWithTagsInfo>>;
