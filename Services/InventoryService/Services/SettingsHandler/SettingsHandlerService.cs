using Datalake.Database.InMemory.Models;
using Datalake.Database.InMemory.Stores;
using Datalake.InventoryService;

namespace Datalake.Server.Services.SettingsHandler;

/// <summary>
/// Сервис обновления настроек, отправляемых клиентам, по изменению данных
/// </summary>
public class SettingsHandlerService(
	DatalakeDataStore dataStore,
	ILogger<SettingsHandlerService> logger) : BackgroundService, IDisposable
{
	/// <inheritdoc/>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// Подписываемся на события
		dataStore.StateChanged += (o, state) =>
		{
			Task.Run(() => WriteStartupFileAsync(state));
		};

		// Обработчики событий
		await WriteStartupFileAsync(dataStore.State);
	}

	/// <summary>
	/// Запись файла с настройками для клиента
	/// </summary>
	public async Task WriteStartupFileAsync(DatalakeDataState state)
	{
		logger.LogDebug("Обновление настроек...");

		try
		{
			var newSettings = state.Settings;
			var version = Program.Version;

			await File.WriteAllLinesAsync(Path.Combine(Program.WebRootPath, "startup.js"),
			[
				"var LOCAL_API = true;",
				$"var KEYCLOAK_DB = '{newSettings.KeycloakHost}';",
				$"var KEYCLOAK_CLIENT = '{newSettings.KeycloakClient}';",
				$"var INSTANCE_NAME = '{newSettings.InstanceName}';",
				$"var VERSION = '{version}';",
			]);

			logger.LogDebug("Настройки обновлены");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка обновления настроек");
		}
	}
}