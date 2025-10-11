namespace Datalake.Inventory.Application.Interfaces.InMemory;

/// <summary>
/// Кэш состояния всех объектов приложения
/// </summary>
public interface IInventoryCache
{
	/// <summary>
	/// Текущий снимок состояния
	/// </summary>
	IInventoryCacheState State { get; }

	/// <summary>
	/// Создание задания на перезагрузку состояния объектов из хранилища
	/// </summary>
	Task RestoreAsync();

	/// <summary>
	/// Создание задания на изменение состояния объектов
	/// </summary>
	/// <param name="updateFunc">Функция, выполняющая изменения и возвращающая новое состояние</param>
	/// <returns></returns>
	Task UpdateAsync(Func<IInventoryCacheState, IInventoryCacheState> updateFunc);

	/// <summary>
	/// Событие успешного завершения изменения состояния
	/// </summary>
	event EventHandler<IInventoryCacheState>? StateChanged;
}
