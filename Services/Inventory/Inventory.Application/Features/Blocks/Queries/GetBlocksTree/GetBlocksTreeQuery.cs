using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksTree;

public record GetBlocksTreeQuery(
	UserAccessValue User) : IQueryRequest<IEnumerable<BlockTreeInfo>>;
