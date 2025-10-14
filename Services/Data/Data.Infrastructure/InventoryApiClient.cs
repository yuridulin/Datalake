using Datalake.Data.Application.Interfaces;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Datalake.Data.Infrastructure;

[Singleton]
public class InventoryApiClient(
	HttpClient client,
	ILogger<InventoryApiClient> logger) : IInventoryApiClient
{
	public async Task<Dictionary<Guid, UserAccessValue>> GetCalculatedAccessAsync(IEnumerable<Guid> guids, CancellationToken cancellationToken = default)
	{
		if (!guids.Any())
			return new();

		logger.LogInformation("Выполняется получение рассчитанных прав доступа из удаленного сервиса");

		try
		{
			var response = await client.PostAsJsonAsync("/api/v1/access/calculated", guids, cancellationToken); // TODO: константа, а не строка!
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadFromJsonAsync<Dictionary<Guid, UserAccessValue>>(cancellationToken);

			return result ?? throw new InfrastructureException("Ответ пуст");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка при получении рассчитанных прав доступа из удаленного сервиса");
			throw;
		}
	}
}
