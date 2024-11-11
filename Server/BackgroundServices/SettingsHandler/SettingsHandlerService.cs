using Datalake.Database;
using Datalake.Database.Constants;
using Datalake.Database.Repositories;
using Datalake.Server.Services.SessionManager;
using Datalake.Server.Services.SessionManager.Models;
using System.Diagnostics;

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
			var lastSystemUpdate = SystemRepository.LastUpdate;
			var lastAccessUpdate = AccessRepository.LastUpdate;

			if (lastSystemUpdate != StoredSystemUpdate || lastAccessUpdate != StoredAccessUpdate)
			{
				var sw = Stopwatch.StartNew();

				try
				{
					using var scope = serviceScopeFactory.CreateScope();
					using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

					if (lastAccessUpdate != StoredAccessUpdate)
					{
						logger.LogInformation("Обновление настроек прав доступа");

						await db.AccessRepository.RebuildUserRightsCacheAsync();
						StoredAccessUpdate = lastAccessUpdate;
					}

					if (lastSystemUpdate != StoredSystemUpdate)
					{
						logger.LogInformation("Обновление системных настроек");

						await WriteStartipFileAsync(db.SystemRepository);
						StoredSystemUpdate = lastSystemUpdate;
					}

					LoadStaticUsers(db.AccessRepository);
				}
				catch (Exception ex)
				{
					logger.LogError("Ошибка при обновлении настроек: {message}", ex.Message);
				}
				finally
				{
					sw.Stop();
					logger.LogInformation("Обновление настроек выполнено за {ms} мс", sw.Elapsed.TotalMilliseconds.ToString("F0"));
				}
			}

			await Task.Delay(1000, stoppingToken);
		}
	}


	private DateTime StoredSystemUpdate = DateTime.MinValue.AddMinutes(1);
	private DateTime StoredAccessUpdate = DateTime.MinValue.AddMinutes(1);

	/// <inheritdoc />
	public async Task WriteStartipFileAsync(SystemRepository systemRepository)
	{
		logger.LogDebug("Обновление настроек, передаваемых веб-клиенту");

		try
		{
			var newSettings = await systemRepository.GetSettingsAsSystemAsync();

			File.WriteAllLines(Path.Combine(Program.WebRootPath, "startup.js"), [
				"var LOCAL_API = true;",
				$"var KEYCLOAK_DB = '{newSettings.EnergoIdHost}';",
				$"var KEYCLOAK_CLIENT = '{newSettings.EnergoIdClient}';",
				$"var INSTANCE_NAME = '{newSettings.InstanceName}'",
			]);

			StoredSystemUpdate = DateFormats.GetCurrentDateTime();
		}
		catch (Exception ex)
		{
			logger.LogError("Ошибка при обновлении настроек, передаваемых веб-клиенту: {message}", ex.Message);
		}
	}

	/// <inheritdoc />
	public void LoadStaticUsers(AccessRepository accessRepository)
	{
		logger.LogDebug("Обновление списка статичных учетных записей");

		try
		{
			var staticUsers = accessRepository
				.GetStaticUsersAsSystemAsync().Result;

			SessionManagerService.StaticAuthRecords = staticUsers
				.Select(x => new AuthSession
				{
					ExpirationTime = DateTime.MaxValue,
					UserGuid = x.Guid,
					Token = x.AuthInfo.Token,
					AuthInfo = x.AuthInfo,
					StaticHost = x.Host
				})
				.ToList();
		}
		catch (Exception ex)
		{
			logger.LogError("Ошибка при обновлении списка статичных учетных записей: {message}", ex.Message);
		}
	}
}
