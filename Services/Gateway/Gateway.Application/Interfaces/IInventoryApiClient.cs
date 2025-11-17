using Datalake.Domain.ValueObjects;

namespace Datalake.Gateway.Application.Interfaces;

/// <summary>
/// Клиент для получения рассчитанных прав доступа
/// </summary>
public interface IInventoryApiClient
{
	/// <summary>
	/// Получение рассчитанных прав доступа
	/// </summary>
	/// <param name="guids">Идентификаторы пользователей</param>
	/// <param name="cancellationToken">Токен отмены</param>
	Task<Dictionary<Guid, UserAccessValue>> GetCalculatedAccessAsync(
		IEnumerable<Guid> guids,
		CancellationToken cancellationToken = default);
}
