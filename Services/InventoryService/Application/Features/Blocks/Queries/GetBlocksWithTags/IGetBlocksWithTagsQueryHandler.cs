using Datalake.InventoryService.Application.Interfaces;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Features.Blocks.Queries.BlocksWithTags;

/// <summary>
/// Запрос информации о блоках со списками тегов
/// </summary>
public interface IGetBlocksWithTagsQueryHandler : IQueryHandler<GetBlocksWithTagsQuery, IEnumerable<BlockWithTagsInfo>>
{
}
