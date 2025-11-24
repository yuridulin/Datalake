using Datalake.Contracts.Models;
using Datalake.Contracts.Models.Blocks;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksWithTags;

/// <summary>
/// Запрос информации о блоках со списками тегов
/// </summary>
public interface IGetBlocksWithTagsHandler : IQueryHandler<GetBlocksWithTagsQuery, IEnumerable<BlockWithTagsInfo>> { }

public class GetBlocksWithTagsHandler(
	IBlocksQueriesService blocksQueriesService) : IGetBlocksWithTagsHandler
{
	public async Task<IEnumerable<BlockWithTagsInfo>> HandleAsync(GetBlocksWithTagsQuery query, CancellationToken ct = default)
	{
		var blocks = await blocksQueriesService.GetAllAsync(ct);
		var blocksTags = await blocksQueriesService.GetNestedTagsAsync(blocks.Select(x => x.Id), ct);

		var tagsByBlock = blocksTags
			.GroupBy(x => x.BlockId)
			.ToDictionary(g => g.Key, g => g.ToArray());

		return blocks
			.Select(block => new BlockWithTagsInfo
			{
				Id = block.Id,
				ParentBlockId = block.ParentId,
				Guid = block.GlobalId,
				Name = block.Name,
				Description = block.Description,
				Tags = tagsByBlock.TryGetValue(block.Id, out var tags) ? tags : [],
				AccessRule = AccessRuleInfo.FromRule(query.User.GetAccessToBlock(block.Id)),
			})
			.ToArray();
	}
}
