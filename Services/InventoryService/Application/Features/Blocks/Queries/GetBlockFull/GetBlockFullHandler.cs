using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Domain.Queries;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Features.Blocks.Queries.GetBlockFull;

/// <summary>
/// Запрос информации о конкретном блоке со всеми его отношениями к другим объектам
/// </summary>
public interface IGetBlockFullHandler : IQueryHandler<GetBlockFullQuery, BlockFullInfo> { }

public class GetBlockFullHandler(
	IBlocksQueriesService blocksQueriesService) : IGetBlockFullHandler
{
	public async Task<BlockFullInfo> HandleAsync(GetBlockFullQuery query, CancellationToken ct = default)
	{
		var data = await blocksQueriesService.GetFullAsync(query.BlockId)
			?? throw Errors.NotFoundBlock(query.BlockId);

		return data;
	}
}
