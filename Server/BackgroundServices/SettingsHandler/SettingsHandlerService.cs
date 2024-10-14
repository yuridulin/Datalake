using Datalake.Database;
using Datalake.Database.Repositories;
using Datalake.Database.Utilities;
using Datalake.Server.Services.SessionManager;
using Datalake.Server.Services.SessionManager.Models;

namespace Datalake.Server.BackgroundServices.SettingsHandler;

/// <summary>
/// Загрузчик настроек из БД
/// </summary>
/// <remarks>
/// Запуск отслеживания настроек БД
/// </remarks>
/// <param name="serviceScopeFactory"></param>
/// <param name="logger"></param>
public class SettingsHandlerService(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<SettingsHandlerService> logger) : BackgroundService, ISettingsUpdater
{
	/// <summary>
	/// Проверка, обновились ли настройки в БД
	/// </summary>
	/// <param name="stoppingToken">Сигнал о остановке выполнения</param>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var lastUpdate = SystemRepository.LastUpdate;

			if (lastUpdate > StoredUpdate)
			{
				logger.LogInformation("Обновление настроек");

				try
				{
					using var scope = serviceScopeFactory.CreateScope();
					using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();


					await WriteStartipFileAsync(db.SystemRepository);
					LoadStaticUsers(db.UsersRepository);

					StoredUpdate = lastUpdate;
				}
				catch (Exception ex)
				{
					logger.LogError("Ошибка при обновлении настроек: {message}", ex.Message);
				}
			}

			await Task.Delay(5000, stoppingToken);
		}
	}


	private DateTime StoredUpdate;

	/// <inheritdoc />
	public async Task WriteStartipFileAsync(SystemRepository systemRepository)
	{
		logger.LogDebug("Обновление настроек, передаваемых веб-клиенту");

		try
		{
			var newSettings = await systemRepository.GetSettingsAsync();

			File.WriteAllLines(Path.Combine(Program.WebRootPath, "startup.js"), [
				"var LOCAL_API = true;",
				$"var KEYCLOAK_DB = '{newSettings.EnergoIdHost}';",
				$"var KEYCLOAK_CLIENT = '{newSettings.EnergoIdClient}';",
			]);

			StoredUpdate = DateTime.Now;
		}
		catch (Exception ex)
		{
			logger.LogError("Ошибка при обновлении настроек, передаваемых веб-клиенту: {message}", ex.Message);
		}
	}

	/// <inheritdoc />
	public void LoadStaticUsers(UsersRepository usersRepository)
	{
		logger.LogDebug("Обновление списка статичных учетных записей");

		try
		{
			SessionManagerService.StaticAuthRecords = usersRepository
				.GetStaticUsers()
				.Select(x => new AuthSession { ExpirationTime = DateTime.MaxValue, User = x.Item1, StaticHost = x.Item2 })
				.ToList();

			StoredUpdate = DateTime.Now;
		}
		catch (Exception ex)
		{
			logger.LogError("Ошибка при обновлении списка статичных учетных записей: {message}", ex.Message);
		}
	}
}
