using Datalake.Data.Application.Models.Tags;

namespace Datalake.Data.Application.Interfaces.Cache;

/// <summary>
/// Хранилище данных о тегах
/// </summary>
public interface ITagsSettingsStore
{
	/// <summary>
	/// Обновление списка тегов
	/// </summary>
	/// <param name="newTags">Новые данные</param>
	Task UpdateAsync(IEnumerable<TagSettingsDto> newTags);

	/// <summary>
	/// Получение данных о теге по его идентификатору
	/// </summary>
	/// <param name="id">Идентификатор</param>
	/// <returns>Данные о теге</returns>
	TagSettingsDto? TryGet(int id);

	/// <summary>
	/// Получение данных о теге по его глобальному идентификатору
	/// </summary>
	/// <param name="id">Глобальный идентификатор</param>
	/// <returns>Данные о теге</returns>
	TagSettingsDto? TryGet(Guid guid);
}
