using Datalake.Contracts.Models.UserGroups;
using Datalake.Contracts.Models.Users;

namespace Datalake.Inventory.Application.Queries;

/// <summary>
/// Запросы, связанные с пользователями
/// </summary>
public interface IUsersQueriesService
{
	/// <summary>
	/// Получение полной информации о учетной записи, включая группы и права доступа
	/// </summary>
	Task<UserInfo?> GetByGuidAsync(
		Guid userGuid,
		CancellationToken ct = default);

	/// <summary>
	/// Запрос полной информации о учетных записях, включая группы и права доступа
	/// </summary>
	Task<List<UserInfo>> GetAsync(
		CancellationToken ct = default);

	/// <summary>
	/// Получение информации о группах, в которых состоит учетная запись
	/// </summary>
	/// <param name="userGuid">Идентификатор учетной записи</param>
	/// <param name="ct">Токен отмены</param>
	Task<List<UserGroupSimpleInfo>> GetGroupsWithMemberAsync(Guid userGuid, CancellationToken ct);
}