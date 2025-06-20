using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory;

/// <summary>
/// 
/// </summary>
public class SourcesMemoryRepository
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<int, Source> _sources = [];

	internal IReadOnlySource[] Sources => _sources.Values.Select(x => (IReadOnlySource)x).ToArray();

	internal IReadOnlyDictionary<int, IReadOnlySource> SourcesDict => Sources.ToDictionary(x => x.Id);

	#endregion


	#region Версия данных для синхронизации

	private string _globalVersion = string.Empty;
	private readonly object _versionLock = new();

	/// <summary>
	/// Событие изменения списка блоков
	/// </summary>
	public event EventHandler<int>? SourcesUpdated;

	#endregion


	#region Инициализация

	/// <summary>
	/// Конструктор репозитория
	/// </summary>
	public SourcesMemoryRepository(
		IServiceScopeFactory serviceScopeFactory)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		InitializeFromDatabase(db).Wait();
		SourcesUpdated?.Invoke(this, 0);
	}

	#endregion


	#region Чтение данных из БД

	private async Task InitializeFromDatabase(DatalakeContext db)
	{
		_sources.Clear();

		var sources = await db.Sources.ToArrayAsync();
		foreach (var source in sources)
			_sources.TryAdd(source.Id, source);

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
		lock (_versionLock)
			_globalVersion = newVersion;
	}

	#endregion


	#region Изменение данных внешними источниками

	#endregion
}

