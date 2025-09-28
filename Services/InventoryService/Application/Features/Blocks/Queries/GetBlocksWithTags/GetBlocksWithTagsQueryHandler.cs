using Datalake.InventoryService.Domain.Queries;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Features.Blocks.Queries.BlocksWithTags;

public class GetBlocksWithTagsQueryHandler(
	IBlocksQueriesService blocksQueriesService) : IGetBlocksWithTagsQueryHandler
{
	public async Task<IEnumerable<BlockWithTagsInfo>> HandleAsync(GetBlocksWithTagsQuery query, CancellationToken ct = default)
	{
		var data = await blocksQueriesService.GetWithTagsAsync();

		return data;
	}
}
