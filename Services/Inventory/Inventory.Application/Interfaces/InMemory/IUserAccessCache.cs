namespace Datalake.Inventory.Application.Interfaces.InMemory;

/// <summary>
/// Хранилище рассчитанных прав доступа данных
/// </summary>
public interface IUserAccessCache
{
	/// <summary>
	/// Текущее состояние
	/// </summary>
	IUserAccessCacheState State { get; }

	/// <summary>
	/// Событие при изменении состояния
	/// </summary>
	event EventHandler<IUserAccessCacheState>? StateChanged;
}