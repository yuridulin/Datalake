using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Api.Models.Blocks;

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
		var data = await blocksQueriesService.GetWithTagsAsync();

		return data;
	}
}
