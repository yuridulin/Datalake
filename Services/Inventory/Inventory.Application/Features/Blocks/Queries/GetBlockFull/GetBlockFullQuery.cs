using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlockFull;

public record GetBlockFullQuery(
	UserAccessEntity User,
	int BlockId) : IQueryRequest<BlockFullInfo>;
