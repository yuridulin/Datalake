using Datalake.Inventory.Application.Models;

namespace Datalake.Inventory.Application.Interfaces;

/// <summary>
/// Хранилище вычисленных прав доступа данных
/// </summary>
public interface IUsersAccessStore
{
	/// <summary>
	/// Текущее состояние
	/// </summary>
	IUserAccessState State { get; }

	/// <summary>
	/// Создание задания обновления состояния вычисленных прав доступа
	/// </summary>
	/// <param name="usersAccess">Вычисленные права доступа</param>
	Task SetAsync(UsersAccessDto usersAccess);

	/// <summary>
	/// Событие при успешном завершении изменения состояния
	/// </summary>
	event EventHandler<IUserAccessState>? StateChanged;
}