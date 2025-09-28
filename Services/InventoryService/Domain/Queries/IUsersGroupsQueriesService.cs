using Datalake.PublicApi.Models.UserGroups;

namespace Datalake.InventoryService.Domain.Queries;

/// <summary>
/// Запросы, связанные с группами пользователей
/// </summary>
public interface IUsersGroupsQueriesService
{
	/// <summary>
	/// Запрос краткой информации о группах пользователей
	/// </summary>
	Task<IEnumerable<UserGroupInfo>> GetAsync();

	/// <summary>
	/// Запрос информации о группах пользователей с правами, подгруппами и участниками
	/// </summary>
	Task<IEnumerable<UserGroupDetailedInfo>> GetWithDetailsAsync();
}