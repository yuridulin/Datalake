using Datalake.Shared.Application.Entities;

namespace Datalake.DataService.Abstractions;

/// <summary>
/// Хранилище рассчитанных прав доступа пользователей
/// </summary>
public interface IAccessStore
{
	/// <summary>
	/// Получение прав доступа для пользователя
	/// </summary>
	/// <param name="guid"></param>
	/// <returns></returns>
	UserAccessEntity? TryGet(Guid guid);

	/// <summary>
	/// Обновление списка сохраненных прав доступа по запросу извне
	/// </summary>
	/// <param name="newAuthInfo">Новые права доступа</param>
	void Update(IEnumerable<UserAccessEntity> newAuthInfo);
}