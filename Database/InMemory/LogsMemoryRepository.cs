using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory;

/// <summary>
/// Репозиторий работы с логами в памяти приложения
/// </summary>
public class LogsMemoryRepository
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<long, Log> _logs = [];

	internal IReadOnlyLog[] Logs
		=> _logs.Values.Select(x => (IReadOnlyLog)x).ToArray();

	internal IReadOnlyDictionary<long, IReadOnlyLog> LogsDict
		=> _logs.ToDictionary(x => x.Key, x => (IReadOnlyLog)x.Value);

	#endregion


	#region Версия данных для синхронизации

	private string _globalVersion = string.Empty;
	private readonly object _versionLock = new();

	/// <summary>
	/// Событие изменения списка логов
	/// </summary>
	public event EventHandler<int>? LogsUpdated;

	#endregion


	#region Инициализация

	/// <summary>
	/// Конструктор репозитория
	/// </summary>
	/// <param name="serviceScopeFactory">Фабрика сервисов</param>
	public LogsMemoryRepository(IServiceScopeFactory serviceScopeFactory)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		InitializeFromDatabase(db).Wait();
		LogsUpdated?.Invoke(this, 0);
	}

	#endregion


	#region Чтение данных из БД

	private async Task InitializeFromDatabase(DatalakeContext db)
	{
		_logs.Clear();

		// Загружаем только последние логи для экономии памяти
		var logs = await db.Logs
			.OrderByDescending(x => x.Date)
			.Take(10000) // Ограничиваем количество логов в памяти
			.ToArrayAsync();
			
		foreach (var log in logs)
			_logs.TryAdd(log.Id, log);

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
		LogsUpdated?.Invoke(this, 0);
	}

	/// <summary>
	/// Добавление нового лога в память
	/// </summary>
	/// <param name="log">Новый лог</param>
	public void AddLog(Log log)
	{
		_logs.TryAdd(log.Id, log);
		
		// Обновляем версию
		var newVersion = DateTime.UtcNow.Ticks.ToString();
		UpdateVersion(newVersion);
		
		LogsUpdated?.Invoke(this, 0);
	}

	#endregion
} 