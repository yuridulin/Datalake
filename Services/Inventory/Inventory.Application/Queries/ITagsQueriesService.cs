using Datalake.Contracts.Models.Tags;
using Datalake.Domain.Enums;

namespace Datalake.Inventory.Application.Queries;

/// <summary>
/// Запросы, связанные с тегами
/// </summary>
public interface ITagsQueriesService
{
	/// <summary>
	/// Запрос информации о тегах
	/// </summary>
	Task<List<TagSimpleInfo>> GetAsync(
		IEnumerable<int>? identifiers = null,
		IEnumerable<Guid>? guids = null,
		TagType? type = null,
		int? sourceId = null,
		CancellationToken ct = default);

	/// <summary>
	/// Запрос информации о тегах и их источниках данных
	/// </summary>
	Task<List<TagWithSettingsInfo>> GetWithSettingsAsync(
		IEnumerable<int>? identifiers = null,
		IEnumerable<Guid>? guids = null,
		TagType? type = null,
		int? sourceId = null,
		CancellationToken ct = default);

	/// <summary>
	/// Запрос полной информации о тегах, их блоках и источниках данных
	/// </summary>
	Task<List<TagBlockRelationInfo>> GetRelationsToBlocksAsync(int tagId, CancellationToken ct = default);

	/// <summary>
	/// Получение тегов-переменных для расчета
	/// </summary>
	/// <param name="tagsId">Идентификаторы тегов-получателей</param>
	/// <param name="ct">Токен отмены</param>
	Task<List<TagInputInfo>> GetInputsAsync(IEnumerable<int> tagsId, CancellationToken ct = default);

	/// <summary>
	/// Получение устанок тегов
	/// </summary>
	/// <param name="tagsId">Идентификаторы тегов-получателей</param>
	/// <param name="ct">Токен отмены</param>
	Task<List<TagThresholdInfo>> GetThresholdsAsync(IEnumerable<int> tagsId, CancellationToken ct = default);
}