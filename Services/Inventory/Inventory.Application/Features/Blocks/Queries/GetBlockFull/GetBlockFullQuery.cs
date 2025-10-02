using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;
using Datalake.Inventory.Api.Models.Blocks;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlockFull;

public record GetBlockFullQuery(
	UserAccessEntity User,
	int BlockId) : IQueryRequest<BlockFullInfo>;
