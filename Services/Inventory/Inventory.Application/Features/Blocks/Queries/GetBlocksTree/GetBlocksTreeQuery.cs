using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;
using Datalake.Inventory.Api.Models.Blocks;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksTree;

public record GetBlocksTreeQuery(
	UserAccessEntity User) : IQueryRequest<IEnumerable<BlockTreeInfo>>;
