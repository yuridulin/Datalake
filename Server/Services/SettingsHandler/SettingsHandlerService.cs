using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Models;
using Datalake.Server.Services.Auth;
using Datalake.Server.Services.Auth.Models;
using System.Threading.Channels;

namespace Datalake.Server.Services.SettingsHandler;

/// <summary>
/// Сервис обновления настроек по изменению данных
/// </summary>
public class SettingsHandlerService(
	DatalakeDataStore dataStore,
	DatalakeDerivedDataStore derivedDataStore,
	ILogger<SettingsHandlerService> logger) : BackgroundService, IDisposable
{
	private readonly object _fileLock = new();
	private readonly object _usersLock = new();

	// Каналы для обработки событий
	private readonly Channel<DatalakeDataState> _stateChannel =
		Channel.CreateUnbounded<DatalakeDataState>();
	private readonly Channel<DatalakeAccessState> _accessChannel =
		Channel.CreateUnbounded<DatalakeAccessState>();

	/// <inheritdoc/>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// Подписываемся на события
		dataStore.StateChanged += OnStateChanged;
		derivedDataStore.AccessChanged += OnAccessChanged;

		// Обработчики событий
		var stateHandler = ProcessStateChangesAsync(stoppingToken);
		var accessHandler = ProcessAccessChangesAsync(stoppingToken);

		await Task.WhenAll(stateHandler, accessHandler);
	}

	/// <inheritdoc/>
	public override void Dispose()
	{
		dataStore.StateChanged -= OnStateChanged;
		derivedDataStore.AccessChanged -= OnAccessChanged;

		GC.SuppressFinalize(this);
	}

	private void OnStateChanged(object? sender, DatalakeDataState state)
		=> _stateChannel.Writer.TryWrite(state);

	private void OnAccessChanged(object? sender, DatalakeAccessState access)
		=> _accessChannel.Writer.TryWrite(access);

	private async Task ProcessStateChangesAsync(CancellationToken ct)
	{
		await foreach (var state in _stateChannel.Reader.ReadAllAsync(ct))
		{
			await Task.Run(() => WriteStartupFile(state), ct);
		}
	}

	private async Task ProcessAccessChangesAsync(CancellationToken ct)
	{
		await foreach (var access in _accessChannel.Reader.ReadAllAsync(ct))
		{
			await Task.Run(() => LoadStaticUsers(dataStore.State, access), ct);
		}
	}

	/// <summary>
	/// Запись файла с настройками для клиента
	/// </summary>
	public void WriteStartupFile(DatalakeDataState state)
	{
		lock (_fileLock)
		{
			logger.LogDebug("Обновление настроек...");
			try
			{
				var newSettings = state.Settings;
				File.WriteAllLines(Path.Combine(Program.WebRootPath, "startup.js"),
				[
					"var LOCAL_API = true;",
					$"var KEYCLOAK_DB = '{newSettings.KeycloakHost}';",
					$"var KEYCLOAK_CLIENT = '{newSettings.KeycloakClient}';",
					$"var INSTANCE_NAME = '{newSettings.InstanceName}'"
				]);
				logger.LogDebug("Настройки обновлены");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Ошибка обновления настроек");
			}
		}
	}

	/// <summary>
	/// Обновление сессий пользователей
	/// </summary>
	/// <param name="state"></param>
	/// <param name="access"></param>
	public void LoadStaticUsers(DatalakeDataState state, DatalakeAccessState access)
	{
		lock (_usersLock)
		{
			logger.LogDebug("Обновление статичных пользователей...");
			try
			{
				var staticUsers = state.Users
					.Where(x => x.Type == PublicApi.Enums.UserType.Static)
					.ToList();

				var sessions = staticUsers
					.Select(user => !access.TryGet(user.Guid, out var rights) ? null : new AuthSession
					{
						ExpirationTime = DateTime.MaxValue,
						UserGuid = user.Guid,
						Token = user.PasswordHash ?? string.Empty,
						AuthInfo = rights,
						StaticHost = user.StaticHost
					})
					.Where(session => session != null)
					.ToList();

				SessionManagerService.StaticAuthRecords = sessions!;
				logger.LogDebug("Пользователи обновлены");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Ошибка обновления пользователей");
			}
		}
	}
}