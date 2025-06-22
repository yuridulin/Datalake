using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory.Repositories;

/*/// <summary>
/// Репозиторий работы с настройками в памяти приложения
/// </summary>
public class SettingsMemoryRepository(
	IServiceScopeFactory serviceScopeFactory,
	Lazy<InMemoryRepositoriesManager> inMemory) : InMemoryRepositoryBase(serviceScopeFactory, inMemory)
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<string, Settings> _settings = [];

	#endregion


	#region Инициализация

	/// <inheritdoc/>
	protected override async Task InitializeFromDatabase(DatalakeContext db)
	{
		_settings.Clear();

		var settings = await db.Settings.ToArrayAsync();
		foreach (var setting in settings)
			_settings.TryAdd(setting.InstanceName, setting);
	}

	#endregion


	#region Чтение данных внешними источниками

	internal IReadOnlySettings[] Settings
		=> _settings.Values.Select(x => (IReadOnlySettings)x).ToArray();

	#endregion


	#region Изменение данных внешними источниками


	#endregion
} */