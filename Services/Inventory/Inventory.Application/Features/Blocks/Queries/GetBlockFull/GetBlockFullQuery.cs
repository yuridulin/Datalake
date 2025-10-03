using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlockFull;

public record GetBlockFullQuery(
	UserAccessEntity User,
	int BlockId) : IQueryRequest<BlockFullInfo>;
