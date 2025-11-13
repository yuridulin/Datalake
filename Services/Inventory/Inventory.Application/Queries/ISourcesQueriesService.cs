using Datalake.Contracts.Public.Models.Sources;

namespace Datalake.Inventory.Application.Queries;

/// <summary>
/// Запросы, связанные с источниками данных
/// </summary>
public interface ISourcesQueriesService
{
	/// <summary>
	/// Запрос информации о источниках без связей
	/// </summary>
	/// <param name="withCustom">Включать ли системные источники в запрос</param>
	/// <param name="ct">Токен отмены</param>
	Task<IEnumerable<SourceInfo>> GetAsync(
		bool withCustom = false,
		CancellationToken ct = default);

	/// <summary>
	/// Запрос информации о источниках вместе со списками зависящих тегов
	/// </summary>
	Task<SourceWithTagsInfo?> GetWithTagsAsync(
		int sourceId,
		CancellationToken ct = default);

	/// <summary>
	/// Запрос информации о источниках вместе со списками зависящих тегов
	/// </summary>
	Task<IEnumerable<SourceWithTagsInfo>> GetWithTagsAsync(
		CancellationToken ct = default);

	/// <summary>
	/// Запрос информации о источниках вместе со списками зависящих тегов
	/// </summary>
	Task<IEnumerable<SourceWithTagsInfo>> GetWithTagsAndSourceTagsAsync(
		CancellationToken ct = default);
}