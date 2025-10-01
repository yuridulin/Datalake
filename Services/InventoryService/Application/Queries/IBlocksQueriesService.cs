using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Queries;

/// <summary>
/// Запросы, связанные с блоками
/// </summary>
public interface IBlocksQueriesService
{
	/// <summary>
	/// Запрос информации о конкретном блоке со всеми его отношениями к другим объектам
	/// </summary>
	Task<BlockFullInfo?> GetFullAsync(
		int blockId,
		CancellationToken ct = default);

	/// <summary>
	/// Запрос информации о блоках со списками тегов
	/// </summary>
	Task<IEnumerable<BlockWithTagsInfo>> GetWithTagsAsync(
		CancellationToken ct = default);
}