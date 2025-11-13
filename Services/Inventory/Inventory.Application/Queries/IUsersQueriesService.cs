using Datalake.Contracts.Public.Models.Users;

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
	Task<IEnumerable<UserInfo>> GetAsync(
		CancellationToken ct = default);
}