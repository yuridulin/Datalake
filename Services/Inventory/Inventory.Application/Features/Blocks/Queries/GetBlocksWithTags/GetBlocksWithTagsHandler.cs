using Datalake.Contracts.Models.Blocks;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksWithTags;

/// <summary>
/// Запрос информации о блоках со списками тегов
/// </summary>
public interface IGetBlocksWithTagsHandler : IQueryHandler<GetBlocksWithTagsQuery, IEnumerable<BlockTreeWithTagsInfo>> { }

public class GetBlocksWithTagsHandler(
	IBlocksQueriesService blocksQueriesService) : IGetBlocksWithTagsHandler
{
	public async Task<IEnumerable<BlockTreeWithTagsInfo>> HandleAsync(GetBlocksWithTagsQuery query, CancellationToken ct = default)
	{
		var blocks = await blocksQueriesService.GetAsync(ct);
		var blockTags = await blocksQueriesService.GetBlockNestedTagsAsync(blocks.Select(x => x.Id), ct);

		var tagsByBlock = blockTags
			.GroupBy(x => x.BlockId)
			.ToDictionary(g => g.Key, g => g.ToArray());

		// TODO: Дополнительная логика авторизации доступа

		return blocks
			.Select(block => new BlockTreeWithTagsInfo
			{
				Id = block.Id,
				Guid = block.Guid,
				Name = block.Name,
				ParentBlockId = block.ParentBlockId,
				Tags = tagsByBlock.TryGetValue(block.Id, out var tags) ? tags : [],
			})
			.ToArray();
	}
}
