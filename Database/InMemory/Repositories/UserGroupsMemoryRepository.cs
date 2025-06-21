using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory.Repositories;

/// <summary>
/// Репозиторий работы с группами пользователей в памяти приложения
/// </summary>
public class UserGroupsMemoryRepository(
	IServiceScopeFactory serviceScopeFactory,
	Lazy<InMemoryRepositoriesManager> inMemory) : InMemoryRepositoryBase(serviceScopeFactory, inMemory)
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<Guid, UserGroup> _userGroups = [];

	#endregion


	#region Инициализация

	/// <inheritdoc/>
	protected override async Task InitializeFromDatabase(DatalakeContext db)
	{
		_userGroups.Clear();

		var userGroups = await db.UserGroups.ToArrayAsync();
		foreach (var userGroup in userGroups)
			_userGroups.TryAdd(userGroup.Guid, userGroup);
	}

	#endregion


	#region Чтение данных внешними источниками

	internal IReadOnlyUserGroup[] UserGroups
		=> _userGroups.Values.Select(x => (IReadOnlyUserGroup)x).ToArray();

	internal IReadOnlyDictionary<Guid, IReadOnlyUserGroup> UserGroupsDict
		=> _userGroups.ToDictionary(x => x.Key, x => (IReadOnlyUserGroup)x.Value);

	#endregion


	#region Изменение данных внешними источниками


	#endregion
} 