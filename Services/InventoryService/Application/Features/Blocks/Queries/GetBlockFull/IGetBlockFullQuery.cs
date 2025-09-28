using Datalake.InventoryService.Application.Interfaces;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Features.Blocks.Queries.BlockFull;

/// <summary>
/// Запрос информации о конкретном блоке со всеми его отношениями к другим объектам
/// </summary>
public interface IGetBlockFullQuery : IQueryHandler<GetBlockFullQuery, BlockFullInfo>
{
}
