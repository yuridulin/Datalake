using Datalake.PublicApi.Models.Users;

namespace Datalake.InventoryService.Domain.Queries;

/// <summary>
/// Запросы, связанные с пользователями
/// </summary>
public interface IUsersQueriesService
{
	/// <summary>
	/// Получение полной информации о учетных записях, включая группы, права доступа и данные для входа
	/// </summary>
	Task<IEnumerable<UserDetailInfo>> GetWithDetailsAsync();

	/// <summary>
	/// Запрос полной информации о учетных записях, включая группы и права доступа
	/// </summary>
	Task<IEnumerable<UserInfo>> GetAsync();
}