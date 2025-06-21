using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory;

/// <summary>
/// Репозиторий работы с настройками в памяти приложения
/// </summary>
public class SettingsMemoryRepository
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<string, Settings> _settings = [];

	internal IReadOnlySettings[] Settings
		=> _settings.Values.Select(x => (IReadOnlySettings)x).ToArray();

	internal IReadOnlySettings? CurrentSettings
		=> _settings.Values.FirstOrDefault();

	#endregion


	#region Версия данных для синхронизации

	private string _globalVersion = string.Empty;
	private readonly object _versionLock = new();

	/// <summary>
	/// Событие изменения настроек
	/// </summary>
	public event EventHandler<int>? SettingsUpdated;

	#endregion


	#region Инициализация

	/// <summary>
	/// Конструктор репозитория
	/// </summary>
	/// <param name="serviceScopeFactory">Фабрика сервисов</param>
	public SettingsMemoryRepository(IServiceScopeFactory serviceScopeFactory)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		InitializeFromDatabase(db).Wait();
		SettingsUpdated?.Invoke(this, 0);
	}

	#endregion


	#region Чтение данных из БД

	private async Task InitializeFromDatabase(DatalakeContext db)
	{
		_settings.Clear();

		var settings = await db.Settings.ToArrayAsync();
		foreach (var setting in settings)
			_settings.TryAdd(setting.InstanceName, setting);

		_globalVersion = DateTime.UtcNow.Ticks.ToString();
	}

	#endregion


	#region Чтение данных внешними источниками

	/// <summary>
	/// Получение текущей версии данных репозитория
	/// </summary>
	public string CurrentVersion
	{
		get { lock (_versionLock) return _globalVersion; }
	}

	/// <summary>
	/// Установка новой версии данных репозитория
	/// </summary>
	/// <param name="newVersion">Значение новой версии</param>
	public void UpdateVersion(string newVersion)
	{
		lock (_versionLock) _globalVersion = newVersion;
	}

	#endregion


	#region Изменение данных внешними источниками

	/// <summary>
	/// Обновление данных из БД
	/// </summary>
	/// <param name="db">Контекст БД</param>
	public async Task RefreshFromDatabase(DatalakeContext db)
	{
		await InitializeFromDatabase(db);
		SettingsUpdated?.Invoke(this, 0);
	}

	#endregion
} 