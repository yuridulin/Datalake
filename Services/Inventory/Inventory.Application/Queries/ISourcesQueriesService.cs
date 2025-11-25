using Datalake.Contracts.Models.Sources;

namespace Datalake.Inventory.Application.Queries;

/// <summary>
/// Запросы, связанные с источниками данных
/// </summary>
public interface ISourcesQueriesService
{
	/// <summary>
	/// Запрос информации о источниках
	/// </summary>
	/// <param name="withCustom">Включать ли системные источники в запрос</param>
	/// <param name="ct">Токен отмены</param>
	Task<SourceWithSettingsInfo[]> GetAllAsync(bool withCustom = false, CancellationToken ct = default);

	/// <summary>
	/// Запрос информации о источнике
	/// </summary>
	/// <param name="sourceId">Идентификатор</param>
	/// <param name="ct">Токен отмены</param>
	Task<SourceWithSettingsInfo?> GetByIdAsync(int sourceId, CancellationToken ct = default);

	/// <summary>
	/// Запрос информации о тегах источника
	/// </summary>
	/// <param name="sourceId">Идентификатор</param>
	/// <param name="ct">Токен отмены</param>
	Task<SourceTagInfo[]> GetSourceTagsAsync(int sourceId, CancellationToken ct);
}