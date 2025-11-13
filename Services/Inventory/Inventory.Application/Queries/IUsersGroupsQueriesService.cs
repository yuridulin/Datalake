using Datalake.Contracts.Public.Models.UserGroups;

namespace Datalake.Inventory.Application.Queries;

/// <summary>
/// Запросы, связанные с группами пользователей
/// </summary>
public interface IUsersGroupsQueriesService
{
	/// <summary>
	/// Запрос краткой информации о группе пользователей
	/// </summary>
	Task<UserGroupInfo?> GetAsync(
		Guid userGroupGuid,
		CancellationToken ct = default);

	/// <summary>
	/// Запрос краткой информации о группах пользователей
	/// </summary>
	Task<IEnumerable<UserGroupInfo>> GetAsync(
		CancellationToken ct = default);

	/// <summary>
	/// Запрос информации о группах пользователей с правами, подгруппами и участниками
	/// </summary>
	Task<UserGroupDetailedInfo?> GetWithDetailsAsync(
		Guid userGroupGuid,
		CancellationToken ct = default);
}