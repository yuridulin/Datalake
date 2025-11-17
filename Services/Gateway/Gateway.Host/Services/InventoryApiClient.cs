using Datalake.Domain.ValueObjects;
using Datalake.Gateway.Application.Interfaces;
using Datalake.Shared.Application.Attributes;

namespace Datalake.Gateway.Host.Services;

/// <summary>
/// Клиент для получения рассчитанных прав доступа по REST + MessagePack
/// </summary>
[Singleton]
public class InventoryApiClient(
	ILogger<InventoryApiClient> logger) : IInventoryApiClient
{
	/// <inheritdoc/>
	public async Task<Dictionary<Guid, UserAccessValue>> GetCalculatedAccessAsync(
		IEnumerable<Guid> guids,
		CancellationToken ct = default)
	{
		logger.LogInformation("Запрос рассчитанных прав доступа");

		throw new NotImplementedException("Не сделано");
	}
}
