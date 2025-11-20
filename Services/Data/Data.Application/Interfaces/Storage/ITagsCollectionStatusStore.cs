using Datalake.Data.Application.Models.Values;

namespace Datalake.Data.Application.Interfaces.Storage;

/// <summary>
/// Хранение последних ошибок по получению значений
/// </summary>
public interface ITagsCollectionStatusStore
{
	/// <summary>
	/// Получение состояния по выбранным тегам
	/// </summary>
	/// <param name="tagsIdentifiers">Идентификаторы тегов</param>
	/// <returns>Ошибки, сопоставленные с идентификаторами тегов</returns>
	IEnumerable<TagCollectionStatus> Get(IEnumerable<int> tagsIdentifiers);

	/// <summary>
	/// Сохранение состояния расчета по выбранному тегу
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="value">Состояние ошибки</param>
	void Set(int tagId, string? value);
}