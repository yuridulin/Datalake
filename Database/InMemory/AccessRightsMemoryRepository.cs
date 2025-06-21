using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory;

/// <summary>
/// Репозиторий работы с правами доступа в памяти приложения
/// </summary>
public class AccessRightsMemoryRepository
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<int, AccessRights> _accessRights = [];

	internal IReadOnlyAccessRights[] AccessRights
		=> _accessRights.Values.Select(x => (IReadOnlyAccessRights)x).ToArray();

	internal IReadOnlyDictionary<int, IReadOnlyAccessRights> AccessRightsDict
		=> _accessRights.ToDictionary(x => x.Key, x => (IReadOnlyAccessRights)x.Value);

	#endregion


	#region Версия данных для синхронизации

	private string _globalVersion = string.Empty;
	private readonly object _versionLock = new();

	/// <summary>
	/// Событие изменения списка прав доступа
	/// </summary>
	public event EventHandler<int>? AccessRightsUpdated;

	#endregion


	#region Инициализация

	/// <summary>
	/// Конструктор репозитория
	/// </summary>
	/// <param name="serviceScopeFactory">Фабрика сервисов</param>
	public AccessRightsMemoryRepository(IServiceScopeFactory serviceScopeFactory)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		InitializeFromDatabase(db).Wait();
		AccessRightsUpdated?.Invoke(this, 0);
	}

	#endregion


	#region Чтение данных из БД

	private async Task InitializeFromDatabase(DatalakeContext db)
	{
		_accessRights.Clear();

		var accessRights = await db.AccessRights.ToArrayAsync();
		foreach (var accessRight in accessRights)
			_accessRights.TryAdd(accessRight.Id, accessRight);

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
		AccessRightsUpdated?.Invoke(this, 0);
	}

	#endregion
} 