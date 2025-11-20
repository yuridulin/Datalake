namespace Datalake.Inventory.Application.Interfaces;

/// <summary>
/// Кэш состояния всех объектов приложения
/// </summary>
public interface IInventoryStore
{
	/// <summary>
	/// Текущий снимок состояния
	/// </summary>
	IInventoryState State { get; }

	/// <summary>
	/// Создание задания на перезагрузку состояния объектов из хранилища
	/// </summary>
	Task RestoreAsync();

	/// <summary>
	/// Создание задания на изменение состояния объектов
	/// </summary>
	/// <param name="updateFunc">Функция, выполняющая изменения и возвращающая новое состояние</param>
	/// <returns></returns>
	Task UpdateAsync(Func<IInventoryState, IInventoryState> updateFunc);

	/// <summary>
	/// Событие успешного завершения изменения состояния
	/// </summary>
	event EventHandler<IInventoryState>? StateChanged;
}
