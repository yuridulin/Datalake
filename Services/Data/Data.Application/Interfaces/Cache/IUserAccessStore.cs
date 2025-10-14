using Datalake.Shared.Application.Entities;

namespace Datalake.Data.Application.Interfaces.Cache;

/// <summary>
/// Хранилище рассчитанных прав доступа пользователей
/// </summary>
public interface IUserAccessStore
{
	/// <summary>
	/// Получение прав доступа для пользователя
	/// </summary>
	/// <param name="guid"></param>
	/// <returns></returns>
	UserAccessValue? TryGet(Guid guid);

	/// <summary>
	/// Обновление списка сохраненных прав доступа по запросу извне
	/// </summary>
	/// <param name="usersAccess">Новые права доступа</param>
	Task UpdateAsync(IDictionary<Guid, UserAccessValue> usersAccess);
}
