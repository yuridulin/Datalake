using Datalake.PublicApi.Models.Tags;

namespace Datalake.DataService.Abstractions;

/// <summary>
/// Хранилище данных о тегах
/// </summary>
public interface ITagsStore
{
	/// <summary>
	/// Обновление списка тегов
	/// </summary>
	/// <param name="newTags">Новые данные</param>
	void Update(IEnumerable<TagCacheInfo> newTags);

	/// <summary>
	/// Получение данных о теге по его идентификатору
	/// </summary>
	/// <param name="id">Идентификатор</param>
	/// <returns>Данные о теге</returns>
	TagCacheInfo? TryGet(int id);

	/// <summary>
	/// Получение данных о теге по его глобальному идентификатору
	/// </summary>
	/// <param name="id">Глобальный идентификатор</param>
	/// <returns>Данные о теге</returns>
	TagCacheInfo? TryGet(Guid guid);

	/// <summary>
	/// Получение данных о тегах источника
	/// </summary>
	/// <param name="sourceId">Идентификатор источника данных</param>
	/// <returns>Список информации о тегах</returns>
	IReadOnlyCollection<TagCacheInfo> GetBySourceId(int sourceId);
}