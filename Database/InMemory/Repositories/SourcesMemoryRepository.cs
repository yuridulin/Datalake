using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory.Repositories;

/// <summary>
/// 
/// </summary>
public class SourcesMemoryRepository(
	IServiceScopeFactory serviceScopeFactory,
	Lazy<InMemoryRepositoriesManager> inMemory) : InMemoryRepositoryBase(serviceScopeFactory, inMemory)
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<int, Source> _sources = [];

	#endregion


	#region Инициализация

	/// <inheritdoc/>
	protected override async Task InitializeFromDatabase(DatalakeContext db)
	{
		_sources.Clear();

		var sources = await db.Sources.ToArrayAsync();
		foreach (var source in sources)
			_sources.TryAdd(source.Id, source);
	}

	#endregion


	#region Чтение данных внешними источниками

	internal IReadOnlySource[] Sources => _sources.Values.Select(x => (IReadOnlySource)x).ToArray();

	internal IReadOnlyDictionary<int, IReadOnlySource> SourcesDict => Sources.ToDictionary(x => x.Id);

	#endregion


	#region Изменение данных внешними источниками


	#endregion
}

