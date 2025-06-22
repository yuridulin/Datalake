using Datalake.Database.InMemory.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Datalake.Database.InMemory;

/// <summary>
/// Менеджер для управления всеми inMemory репозиториями
/// </summary>
public class InMemoryRepositoriesManager
{
	#region Репозитории

	/*/// <summary>
	/// Репозиторий блоков
	/// </summary>
	public BlocksMemoryRepository Blocks { get; }*/

	/// <summary>
	/// Репозиторий тегов
	/// </summary>
	public TagsMemoryRepository TagsRepository { get; }

	/*/// <summary>
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
	public LogsMemoryRepository Logs { get; }*/

	private DatalakeStateHolder _stateHolder;

	#endregion


	#region Инициализация

	/// <summary>
	/// Конструктор менеджера
	/// </summary>
	/// <param name="serviceScopeFactory">Фабрика сервисов</param>
	public InMemoryRepositoriesManager(IServiceScopeFactory serviceScopeFactory)
	{
		using var serviceScope = serviceScopeFactory.CreateScope();
		_stateHolder = serviceScope.ServiceProvider.GetRequiredService<DatalakeStateHolder>();

		// Инициализируем репозитории в правильном порядке для избежания циклических зависимостей
		/*Blocks = new BlocksMemoryRepository(stateHolder);
		Sources = new SourcesMemoryRepository(stateHolder);*/
		TagsRepository = new TagsMemoryRepository(_stateHolder);
		/*Users = new UsersMemoryRepository(stateHolder);
		UserGroups = new UserGroupsMemoryRepository(stateHolder);
		AccessRights = new AccessRightsMemoryRepository(stateHolder);
		Settings = new SettingsMemoryRepository(stateHolder);
		Logs = new LogsMemoryRepository(stateHolder);*/

		// Подписываемся на события обновления для синхронизации
		SubscribeToEvents();
	}

	#endregion


	#region События

	private void SubscribeToEvents()
	{
		_stateHolder.StateChanged += OnRepositoryUpdated;
	}

	private void OnRepositoryUpdated(object? holder, DatalakeState newState)
	{
		// Здесь можно добавить логику для обработки обновлений репозиториев
		// Например, логирование, уведомления и т.д.
	}

	#endregion
} 