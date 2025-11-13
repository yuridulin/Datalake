using Datalake.Contracts.Public.Models.Tags;

namespace Datalake.Inventory.Application.Queries;

/// <summary>
/// Запросы, связанные с тегами
/// </summary>
public interface ITagsQueriesService
{
	/// <summary>
	/// Запрос полной информации о тегах, их блоках и источниках данных
	/// </summary>
	Task<TagFullInfo?> GetWithDetailsAsync(
		int tagId,
		CancellationToken ct = default);

	/// <summary>
	/// Запрос информации о тегах и их источниках данных
	/// </summary>
	Task<IEnumerable<TagInfo>> GetAsync(
		IEnumerable<int>? identifiers,
		IEnumerable<Guid>? guids,
		int? sourceId,
		CancellationToken ct = default);
}