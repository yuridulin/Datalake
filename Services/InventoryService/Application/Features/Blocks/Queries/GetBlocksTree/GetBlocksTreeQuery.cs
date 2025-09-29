using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Features.Blocks.Queries.GetBlocksTree;

public record GetBlocksTreeQuery(
	UserAccessEntity User) : IQuery<IEnumerable<BlockTreeInfo>>;
