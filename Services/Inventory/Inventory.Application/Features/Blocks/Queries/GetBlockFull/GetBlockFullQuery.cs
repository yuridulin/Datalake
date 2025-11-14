using Datalake.Contracts.Models.Blocks;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlockFull;

public record GetBlockFullQuery(
	UserAccessValue User,
	int BlockId) : IQueryRequest<BlockFullInfo>;
