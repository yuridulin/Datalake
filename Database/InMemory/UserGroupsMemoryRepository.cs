using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory;

/// <summary>
/// Репозиторий работы с группами пользователей в памяти приложения
/// </summary>
public class UserGroupsMemoryRepository
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<Guid, UserGroup> _userGroups = [];

	internal IReadOnlyUserGroup[] UserGroups
		=> _userGroups.Values.Select(x => (IReadOnlyUserGroup)x).ToArray();

	internal IReadOnlyDictionary<Guid, IReadOnlyUserGroup> UserGroupsDict
		=> _userGroups.ToDictionary(x => x.Key, x => (IReadOnlyUserGroup)x.Value);

	#endregion


	#region Версия данных для синхронизации

	private string _globalVersion = string.Empty;
	private readonly object _versionLock = new();

	/// <summary>
	/// Событие изменения списка групп пользователей
	/// </summary>
	public event EventHandler<int>? UserGroupsUpdated;

	#endregion


	#region Инициализация

	/// <summary>
	/// Конструктор репозитория
	/// </summary>
	/// <param name="serviceScopeFactory">Фабрика сервисов</param>
	public UserGroupsMemoryRepository(IServiceScopeFactory serviceScopeFactory)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		InitializeFromDatabase(db).Wait();
		UserGroupsUpdated?.Invoke(this, 0);
	}

	#endregion


	#region Чтение данных из БД

	private async Task InitializeFromDatabase(DatalakeContext db)
	{
		_userGroups.Clear();

		var userGroups = await db.UserGroups.ToArrayAsync();
		foreach (var userGroup in userGroups)
			_userGroups.TryAdd(userGroup.Guid, userGroup);

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
		UserGroupsUpdated?.Invoke(this, 0);
	}

	#endregion
} 