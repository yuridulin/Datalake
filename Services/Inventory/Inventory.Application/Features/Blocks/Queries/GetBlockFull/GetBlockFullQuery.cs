using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlockFull;

public record GetBlockFullQuery(
	UserAccessValue User,
	int BlockId) : IQueryRequest<BlockFullInfo>;
