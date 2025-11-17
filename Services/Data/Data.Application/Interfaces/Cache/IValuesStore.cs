using Datalake.Domain.Entities;

namespace Datalake.Data.Application.Interfaces.Cache;

/// <summary>
/// Хранилище текущих значений тегов
/// </summary>
public interface IValuesStore
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
	/// Попытка записи новых значений. В процессе проходит проверка на новизну. Если значение не новее, то записи не будет.
	/// Этот метод вызывает событие <see cref="ValuesChanged"/> по завершению при наличии изменений.
	/// </summary>
	/// <param name="incomingValues">Значения для записи</param>
	/// <returns>Флаг, есть ли новые значения</returns>
	bool TryUpdate(IReadOnlyList<TagValue> incomingValues);

	/// <summary>
	/// Событие, возникающее при изменении одного или нескольких значений тегов
	/// </summary>
	event EventHandler<ValuesChangedEventArgs>? ValuesChanged;
}

/// <summary>
/// Аргументы события изменения значений тегов
/// </summary>
/// <param name="changedTagIds">Идентификаторы измененных тегов</param>
public class ValuesChangedEventArgs(IReadOnlyList<int> changedTagIds) : EventArgs
{
	/// <summary>
	/// Идентификаторы тегов, значения которых изменились
	/// </summary>
	public IReadOnlyList<int> ChangedTags { get; } = changedTagIds;
}