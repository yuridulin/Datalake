using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory.Repositories;

/*/// <summary>
/// Репозиторий работы с пользователями в памяти приложения
/// </summary>
public class UsersMemoryRepository(
	IServiceScopeFactory serviceScopeFactory,
	Lazy<InMemoryRepositoriesManager> inMemory) : InMemoryRepositoryBase(serviceScopeFactory, inMemory)
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<Guid, User> _users = [];

	#endregion


	#region Инициализация

	/// <inheritdoc/>
	protected override async Task InitializeFromDatabase(DatalakeContext db)
	{
		_users.Clear();

		var users = await db.Users.ToArrayAsync();
		foreach (var user in users)
			_users.TryAdd(user.Guid, user);
	}

	#endregion


	#region Чтение данных внешними источниками

	internal IReadOnlyUser[] Users
		=> _users.Values.Select(x => (IReadOnlyUser)x).ToArray();

	internal IReadOnlyDictionary<Guid, IReadOnlyUser> UsersDict
		=> _users.ToDictionary(x => x.Key, x => (IReadOnlyUser)x.Value);

	#endregion


	#region Изменение данных внешними источниками



	#endregion
} */