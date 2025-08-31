using Datalake.Database.InMemory.Stores;
using Datalake.Database.InMemory.Stores.Derived;
using Datalake.Server.Services.Auth;
using Datalake.Server.Services.SettingsHandler;

namespace Datalake.Server.Services.Initialization;

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

		logger.LogInformation("Созданы экземпляры:\r\n{names}", string.Join(",\r\n", new[]
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
