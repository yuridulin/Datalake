using Datalake.PublicApi.Models.Sources;

namespace Datalake.InventoryService.Domain.Queries;

/// <summary>
/// Запросы, связанные с источниками данных
/// </summary>
public interface ISourceQueriesService
{
	/// <summary>
	/// Запрос информации о источниках без связей
	/// </summary>
	/// <param name="withCustom">Включать ли системные источники в запрос</param>
	Task<IEnumerable<SourceInfo>> GetAsync(bool withCustom = false);

	/// <summary>
	/// Запрос информации о источниках вместе со списками зависящих тегов
	/// </summary>
	Task<IEnumerable<SourceWithTagsInfo>> GetWithTagsAsync();

	/// <summary>
	/// Запрос информации о источниках вместе со списками зависящих тегов
	/// </summary>
	Task<IEnumerable<SourceWithTagsInfo>> GetWithTagsAndSourceTagsAsync();
}