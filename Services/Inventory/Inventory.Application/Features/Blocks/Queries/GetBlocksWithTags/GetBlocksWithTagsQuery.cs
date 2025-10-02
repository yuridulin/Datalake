using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;
using Datalake.Inventory.Api.Models.Blocks;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksWithTags;

public record GetBlocksWithTagsQuery(
	UserAccessEntity User) : IQueryRequest<IEnumerable<BlockWithTagsInfo>>;
