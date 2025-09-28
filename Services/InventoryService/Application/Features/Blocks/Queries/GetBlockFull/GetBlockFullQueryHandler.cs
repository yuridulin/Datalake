using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Domain.Queries;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Features.Blocks.Queries.BlockFull;

public class GetBlockFullQueryHandler(
	IBlocksQueriesService blocksQueriesService) : IGetBlockFullQuery
{
	public async Task<BlockFullInfo> HandleAsync(GetBlockFullQuery query, CancellationToken ct = default)
	{
		var data = await blocksQueriesService.GetFullAsync(query.BlockId)
			?? throw Errors.NotFoundBlock(query.BlockId);

		return data;
	}
}
