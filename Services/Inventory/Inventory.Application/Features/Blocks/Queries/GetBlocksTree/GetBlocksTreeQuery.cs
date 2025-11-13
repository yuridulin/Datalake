using Datalake.Contracts.Public.Models.Blocks;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksTree;

public record GetBlocksTreeQuery(
	UserAccessValue User) : IQueryRequest<IEnumerable<BlockTreeInfo>>;
