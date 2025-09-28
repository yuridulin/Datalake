using Datalake.InventoryService.Domain.Queries;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Features.Blocks.Queries.BlocksTree;

public class GetBlockTreeQueryHandler(
	IBlocksQueriesService blocksQueriesService) : IGetBlocksTreeQuery
{
	public async Task<IEnumerable<BlockTreeInfo>> HandleAsync(GetBlocksTreeQuery query, CancellationToken ct = default)
	{
		var data = await blocksQueriesService.GetTreeAsync();

		return data;
	}
}
