using Datalake.InventoryService.InMemory.Stores;
using Datalake.InventoryService.Services.Auth;
using Datalake.InventoryService.Services.SettingsHandler;

namespace Datalake.InventoryService.Services.Initialization;

/// <summary>
/// Загрузка данных после запуска приложения
/// </summary>
public class LoaderService(
	ILogger<LoaderService> logger,
	DatalakeDataStore dataStore,
	DatalakeAccessStore accessStore,
	AuthenticationService authenticationService,
	SettingsHandlerService settingsHandlerService) : IHostedService
{
	/// <inheritdoc/>
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Общий запуск");
		logger.LogInformation("Созданы экземпляры:\n\t{names}", string.Join(",\n\t", new[]
		{
			dataStore.ToString(),
			accessStore.ToString(),
			authenticationService.ToString(),
			settingsHandlerService.ToString(),
		}));

		await dataStore.ReloadStateAsync();

		logger.LogInformation("Общий запуск завершен");
	}

	/// <inheritdoc/>
	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
