using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory.Repositories;

/*/// <summary>
/// Репозиторий работы с правами доступа в памяти приложения
/// </summary>
public class AccessRightsMemoryRepository(
	IServiceScopeFactory serviceScopeFactory,
	Lazy<InMemoryRepositoriesManager> inMemory) : InMemoryRepositoryBase(serviceScopeFactory, inMemory)
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<int, AccessRights> _accessRights = [];

	#endregion


	#region Инициализация

	/// <inheritdoc/>
	protected override async Task InitializeFromDatabase(DatalakeContext db)
	{
		_accessRights.Clear();

		var accessRights = await db.AccessRights.ToArrayAsync();
		foreach (var accessRight in accessRights)
			_accessRights.TryAdd(accessRight.Id, accessRight);
	}
	#endregion


	#region Чтение данных внешними источниками

	internal IReadOnlyAccessRights[] AccessRights
		=> _accessRights.Values.Select(x => (IReadOnlyAccessRights)x).ToArray();

	internal IReadOnlyDictionary<int, IReadOnlyAccessRights> AccessRightsDict
		=> _accessRights.ToDictionary(x => x.Key, x => (IReadOnlyAccessRights)x.Value);

	#endregion


	#region Изменение данных внешними источниками



	#endregion
} */