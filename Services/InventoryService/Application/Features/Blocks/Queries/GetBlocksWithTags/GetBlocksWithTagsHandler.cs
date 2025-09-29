using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Domain.Queries;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Features.Blocks.Queries.GetBlocksWithTags;

/// <summary>
/// Запрос информации о блоках со списками тегов
/// </summary>
public interface IGetBlocksWithTagsHandler : IQueryHandler<GetBlocksWithTagsQuery, IEnumerable<BlockWithTagsInfo>> { }

public class GetBlocksWithTagsHandler(
	IBlocksQueriesService blocksQueriesService) : IGetBlocksWithTagsHandler
{
	public async Task<IEnumerable<BlockWithTagsInfo>> HandleAsync(GetBlocksWithTagsQuery query, CancellationToken ct = default)
	{
		var data = await blocksQueriesService.GetWithTagsAsync();

		return data;
	}
}
