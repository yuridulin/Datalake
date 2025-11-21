using Datalake.Contracts.Models.Blocks;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksTree;

public interface IGetBlocksTreeHandler : IQueryHandler<GetBlocksTreeQuery, IEnumerable<BlockTreeInfo>> { }

public class GetBlocksTreeHandler(
	IBlocksQueriesService blocksQueriesService) : IGetBlocksTreeHandler
{
	public async Task<IEnumerable<BlockTreeInfo>> HandleAsync(GetBlocksTreeQuery query, CancellationToken ct = default)
	{
		var data = await blocksQueriesService.GetAsync(ct);

		return data;
	}
}
