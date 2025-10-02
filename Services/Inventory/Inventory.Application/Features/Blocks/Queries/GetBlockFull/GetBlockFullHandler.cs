using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Api.Models.Blocks;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlockFull;

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
			?? throw InventoryNotFoundException.NotFoundBlock(query.BlockId);

		return data;
	}
}
