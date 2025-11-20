using Datalake.Domain.ValueObjects;

namespace Datalake.Shared.Application.Interfaces.AccessRules;

/// <summary>
/// Хранилище рассчитанных прав доступа пользователей
/// </summary>
public interface IUsersAccessStore
{
	/// <summary>
	/// Получение рассчитанных прав доступа пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	UserAccessValue? Get(Guid userGuid);

	/// <summary>
	/// Обновление рассчитанных прав доступа пользователей
	/// </summary>
	/// <param name="access">Новые права доступа, сгруппированные по пользователям</param>
	void Set(IDictionary<Guid, UserAccessValue> access);
}
