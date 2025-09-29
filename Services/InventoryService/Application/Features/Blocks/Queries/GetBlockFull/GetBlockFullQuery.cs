using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Features.Blocks.Queries.GetBlockFull;

public record GetBlockFullQuery(
	UserAccessEntity User,
	int BlockId) : IQuery<BlockFullInfo>;
