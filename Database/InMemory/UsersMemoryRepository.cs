using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory;

/// <summary>
/// Репозиторий работы с пользователями в памяти приложения
/// </summary>
public class UsersMemoryRepository
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<Guid, User> _users = [];

	internal IReadOnlyUser[] Users
		=> _users.Values.Select(x => (IReadOnlyUser)x).ToArray();

	internal IReadOnlyDictionary<Guid, IReadOnlyUser> UsersDict
		=> _users.ToDictionary(x => x.Key, x => (IReadOnlyUser)x.Value);

	#endregion


	#region Версия данных для синхронизации

	private string _globalVersion = string.Empty;
	private readonly object _versionLock = new();

	/// <summary>
	/// Событие изменения списка пользователей
	/// </summary>
	public event EventHandler<int>? UsersUpdated;

	#endregion


	#region Инициализация

	/// <summary>
	/// Конструктор репозитория
	/// </summary>
	/// <param name="serviceScopeFactory">Фабрика сервисов</param>
	public UsersMemoryRepository(IServiceScopeFactory serviceScopeFactory)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		InitializeFromDatabase(db).Wait();
		UsersUpdated?.Invoke(this, 0);
	}

	#endregion


	#region Чтение данных из БД

	private async Task InitializeFromDatabase(DatalakeContext db)
	{
		_users.Clear();

		var users = await db.Users.ToArrayAsync();
		foreach (var user in users)
			_users.TryAdd(user.Guid, user);

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
		UsersUpdated?.Invoke(this, 0);
	}

	#endregion
} 