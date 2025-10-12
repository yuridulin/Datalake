namespace Datalake.Inventory.Application.Interfaces;

/// <summary>
/// Оркестратор обновления зависимых данных
/// </summary>
public interface IUserAccessSynchronizationService
{
	/// <summary>
	/// Настройка процесса синхронизации
	/// </summary>
	void Start();
}
