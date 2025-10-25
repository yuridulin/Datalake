using Datalake.Domain.Entities;

namespace Datalake.Data.Application.Interfaces.Cache;

/// <summary>
/// Хранилище текущих значений тегов
/// </summary>
public interface ICurrentValuesStore
{
	/// <summary>
	/// Обновление текущих значений из БД
	/// </summary>
	Task ReloadValuesAsync(IEnumerable<TagValue> values);

	/// <summary>
	/// Получение значения по идентификатору
	/// </summary>
	/// <param name="id">Локальный идентификатор тега</param>
	/// <returns>Значение, если существует</returns>
	TagValue? TryGet(int id);

	/// <summary>
	/// Получение значений по идентификаторам в виде словаря
	/// </summary>
	/// <param name="identifiers">Локальные идентификаторы тегов</param>
	/// <returns>Значения, если есть, сопоставленные с идентификаторами</returns>
	Dictionary<int, TagValue?> GetByIdentifiers(int[] identifiers);

	/// <summary>
	/// Проверка, является ли значение новым
	/// </summary>
	/// <param name="id">Локальный идентификатор тега</param>
	/// <param name="incomingValue">Значение для проверки</param>
	/// <returns></returns>
	bool IsNew(int id, TagValue incomingValue);

	/// <summary>
	/// Попытка записи нового значения. В процессе проходит проверка на новизну. Если значение не новее, то записи не будет.
	/// </summary>
	/// <param name="id">Локальный идентификатор тега</param>
	/// <param name="incomingValue">Значение для записи</param>
	/// <returns>Флаг, является ли значение новым</returns>
	bool TryUpdate(int id, TagValue incomingValue);
}
