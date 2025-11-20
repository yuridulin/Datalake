namespace Datalake.Gateway.Application.Interfaces.Storage;

/// <summary>
/// Кэш последних появлений пользователей
/// </summary>
public interface IUsersActivityStore
{
	/// <summary>
	/// Получение времени последнего визита пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	DateTime? Get(Guid userGuid);

	/// <summary>
	/// Запись времени последнего визита пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	void Set(Guid guid);
}