using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Domain.Queries;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Features.Blocks.Queries.GetBlocksTree;

/// <summary>
/// Запрос информации о блоках со списками тегов IGetBlocksTreeQuery
/// </summary>
public interface IGetBlocksTreeHandler : IQueryHandler<GetBlocksTreeQuery, IEnumerable<BlockTreeInfo>> { }

public class GetBlocksTreeHandler(
	IBlocksQueriesService blocksQueriesService) : IGetBlocksTreeHandler
{
	public async Task<IEnumerable<BlockTreeInfo>> HandleAsync(GetBlocksTreeQuery query, CancellationToken ct = default)
	{
		var data = await blocksQueriesService.GetTreeAsync();

		return data;
	}
}
