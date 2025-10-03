using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksTree;

public record GetBlocksTreeQuery(
	UserAccessEntity User) : IQueryRequest<IEnumerable<BlockTreeInfo>>;
