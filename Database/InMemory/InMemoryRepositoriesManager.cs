using Datalake.Database.InMemory.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Datalake.Database.InMemory;

/// <summary>
/// Менеджер для управления всеми inMemory репозиториями
/// </summary>
public class InMemoryRepositoriesManager
{
	#region Репозитории

	/// <summary>
	/// Репозиторий блоков
	/// </summary>
	public BlocksMemoryRepository Blocks { get; }

	/// <summary>
	/// Репозиторий тегов
	/// </summary>
	public TagsMemoryRepository Tags { get; }

	/// <summary>
	/// Репозиторий источников
	/// </summary>
	public SourcesMemoryRepository Sources { get; }

	/// <summary>
	/// Репозиторий пользователей
	/// </summary>
	public UsersMemoryRepository Users { get; }

	/// <summary>
	/// Репозиторий групп пользователей
	/// </summary>
	public UserGroupsMemoryRepository UserGroups { get; }

	/// <summary>
	/// Репозиторий прав доступа
	/// </summary>
	public AccessRightsMemoryRepository AccessRights { get; }

	/// <summary>
	/// Репозиторий настроек
	/// </summary>
	public SettingsMemoryRepository Settings { get; }

	/// <summary>
	/// Репозиторий логов
	/// </summary>
	public LogsMemoryRepository Logs { get; }

	#endregion


	#region Инициализация

	/// <summary>
	/// Конструктор менеджера
	/// </summary>
	/// <param name="serviceScopeFactory">Фабрика сервисов</param>
	public InMemoryRepositoriesManager(IServiceScopeFactory serviceScopeFactory)
	{
		// Инициализируем репозитории в правильном порядке для избежания циклических зависимостей
		Blocks = new BlocksMemoryRepository(serviceScopeFactory, new Lazy<InMemoryRepositoriesManager>(this));
		Sources = new SourcesMemoryRepository(serviceScopeFactory, new Lazy<InMemoryRepositoriesManager>(this));
		Tags = new TagsMemoryRepository(serviceScopeFactory, new Lazy<InMemoryRepositoriesManager>(this));
		Users = new UsersMemoryRepository(serviceScopeFactory, new Lazy<InMemoryRepositoriesManager>(this));
		UserGroups = new UserGroupsMemoryRepository(serviceScopeFactory, new Lazy<InMemoryRepositoriesManager>(this));
		AccessRights = new AccessRightsMemoryRepository(serviceScopeFactory, new Lazy<InMemoryRepositoriesManager>(this));
		Settings = new SettingsMemoryRepository(serviceScopeFactory, new Lazy<InMemoryRepositoriesManager>(this));
		Logs = new LogsMemoryRepository(serviceScopeFactory, new Lazy<InMemoryRepositoriesManager>(this));

		// Подписываемся на события обновления для синхронизации
		SubscribeToEvents();
	}

	#endregion


	#region События

	private void SubscribeToEvents()
	{
		// Подписываемся на события обновления репозиториев
		Blocks.Updated += OnRepositoryUpdated;
		Tags.Updated += OnRepositoryUpdated;
		Sources.Updated += OnRepositoryUpdated;
		Users.Updated += OnRepositoryUpdated;
		UserGroups.Updated += OnRepositoryUpdated;
		AccessRights.Updated += OnRepositoryUpdated;
		Settings.Updated += OnRepositoryUpdated;
		Logs.Updated += OnRepositoryUpdated;
	}

	private void OnRepositoryUpdated(object? sender, int e)
	{
		// Здесь можно добавить логику для обработки обновлений репозиториев
		// Например, логирование, уведомления и т.д.
	}

	#endregion


	#region Обновление данных

	/// <summary>
	/// Обновление всех репозиториев из БД
	/// </summary>
	/// <param name="db">Контекст БД</param>
	public async Task RefreshAllFromDatabase(DatalakeContext db)
	{
		await Task.WhenAll(
			Blocks.RefreshFromDatabase(db),
			Tags.RefreshFromDatabase(db),
			Sources.RefreshFromDatabase(db),
			Users.RefreshFromDatabase(db),
			UserGroups.RefreshFromDatabase(db),
			AccessRights.RefreshFromDatabase(db),
			Settings.RefreshFromDatabase(db),
			Logs.RefreshFromDatabase(db)
		);
	}

	/// <summary>
	/// Получение общей версии всех репозиториев
	/// </summary>
	/// <returns>Объединенная версия всех репозиториев</returns>
	public string GetCombinedVersion()
	{
		var versions = new[]
		{
			Blocks.CurrentVersion,
			Tags.CurrentVersion,
			Sources.CurrentVersion,
			Users.CurrentVersion,
			UserGroups.CurrentVersion,
			AccessRights.CurrentVersion,
			Settings.CurrentVersion,
			Logs.CurrentVersion,
		};

		// Объединяем версии в одну строку
		return string.Join("|", versions);
	}

	#endregion


	#region Утилиты

	/// <summary>
	/// Получение статистики по репозиториям
	/// </summary>
	/// <returns>Словарь с количеством элементов в каждом репозитории</returns>
	public Dictionary<string, int> GetRepositoriesStatistics()
	{
		return new Dictionary<string, int>
		{
			["Blocks"] = Blocks.Blocks.Length,
			["Tags"] = Tags.Tags.Length,
			["Sources"] = Sources.Sources.Length,
			["Users"] = Users.Users.Length,
			["UserGroups"] = UserGroups.UserGroups.Length,
			["AccessRights"] = AccessRights.AccessRights.Length,
			["Settings"] = Settings.Settings.Length,
			["Logs"] = Logs.Logs.Length,
		};
	}

	#endregion
} 