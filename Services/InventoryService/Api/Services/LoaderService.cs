using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Cache.UserAccess;
using Datalake.PrivateApi.Attributes;

namespace Datalake.InventoryService.Api.Services;

/// <summary>
/// Загрузка данных после запуска приложения
/// </summary>
[Singleton]
public class LoaderService(
	ILogger<LoaderService> logger,
	IInventoryCache inventoryCache,
	IUserAccessCache userAccessCache,
	AuthenticationService authenticationService,
	SettingsHandlerService settingsHandlerService) : IHostedService
{
	/// <inheritdoc/>
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Общий запуск");
		logger.LogInformation("Созданы экземпляры:\n\t{names}", string.Join(",\n\t", new[]
		{
			inventoryCache.ToString(),
			userAccessCache.ToString(),
			authenticationService.ToString(),
			settingsHandlerService.ToString(),
		}));

		await inventoryCache.RestoreAsync();

		logger.LogInformation("Общий запуск завершен");
	}

	/// <inheritdoc/>
	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
