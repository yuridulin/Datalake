using Datalake.Contracts.Models.Blocks;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlockFull;

/// <summary>
/// Запрос информации о конкретном блоке со всеми его отношениями к другим объектам
/// </summary>
public interface IGetBlockFullHandler : IQueryHandler<GetBlockFullQuery, BlockDetailedInfo> { }

public class GetBlockFullHandler(
	IBlocksQueriesService blocksQueriesService) : IGetBlockFullHandler
{
	public async Task<BlockDetailedInfo> HandleAsync(GetBlockFullQuery query, CancellationToken ct = default)
	{
		var data = await blocksQueriesService.GetWithParentsAsync(query.BlockId, ct);
		if (data.Length == 0)
			throw InventoryNotFoundException.NotFoundBlock(query.BlockId);

		var block = data[0];
		var result = new BlockDetailedInfo
		{
			Id = block.Id,
			Guid = block.Guid,
			Name = block.Name,
			Adults = data.Skip(1).ToArray(),
			Tags = await blocksQueriesService.GetBlockNestedTagsAsync([query.BlockId], ct),

		};

		return result;
	}
}
