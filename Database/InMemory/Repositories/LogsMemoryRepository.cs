using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory.Repositories;

/// <summary>
/// Репозиторий работы с логами в памяти приложения
/// </summary>
public class LogsMemoryRepository(
	IServiceScopeFactory serviceScopeFactory,
	Lazy<InMemoryRepositoriesManager> inMemory) : InMemoryRepositoryBase(serviceScopeFactory, inMemory)
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<long, Log> _logs = [];

	#endregion


	#region Инициализация

	/// <inheritdoc/>
	protected override async Task InitializeFromDatabase(DatalakeContext db)
	{
		_logs.Clear();

		// Загружаем только последние логи для экономии памяти
		var logs = await db.Logs
			.OrderByDescending(x => x.Date)
			.Take(10000) // Ограничиваем количество логов в памяти
			.ToArrayAsync();
			
		foreach (var log in logs)
			_logs.TryAdd(log.Id, log);
	}

	#endregion


	#region Чтение данных внешними источниками

	internal IReadOnlyLog[] Logs
		=> _logs.Values.Select(x => (IReadOnlyLog)x).ToArray();

	internal IReadOnlyDictionary<long, IReadOnlyLog> LogsDict
		=> _logs.ToDictionary(x => x.Key, x => (IReadOnlyLog)x.Value);

	#endregion


	#region Изменение данных внешними источниками

	/// <summary>
	/// Добавление нового лога в память
	/// </summary>
	/// <param name="log">Новый лог</param>
	public void AddLog(Log log)
	{
		_logs.TryAdd(log.Id, log);

		Trigger();
	}

	#endregion
} 