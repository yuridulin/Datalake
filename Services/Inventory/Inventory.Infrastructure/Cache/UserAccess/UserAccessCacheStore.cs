using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Infrastructure.Cache.Inventory;
using Datalake.Shared.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Infrastructure.Cache.UserAccess;

public class UserAccessCacheStore : IUserAccessCache
{
	private readonly ILogger<UserAccessCacheStore> _logger;
	private readonly UserAccessStateFactory _accessResolverService;
	private readonly Lock _lock = new();
	private UserAccessState _state;
	private long _lastProcessingDataVersion;

	public UserAccessCacheStore(
		InventoryCacheStore dataStore,
		UserAccessStateFactory accessResolverService,
		ILogger<UserAccessCacheStore> logger)
	{
		_logger = logger;
		_accessResolverService = accessResolverService;

		// Вычисляем изначальное состояние сразу же
		lock (_lock)
		{
			_lastProcessingDataVersion = dataStore.State.Version;
			_state = Measures.Measure(() => CreateDerivedState(dataStore.State), _logger, $"{nameof(UserAccessCacheStore)}");
			_logger.LogInformation("Инициализация зависимых данных для версии {Version}", _lastProcessingDataVersion);
		}

		// Подписываемся на будущие изменения
		dataStore.StateChanged += (_, newState) =>
		{
			// Атомарно проверяем и обновляем версию
			long currentVersion = Interlocked.Read(ref _lastProcessingDataVersion);
			if (newState.Version > currentVersion)
			{
				// Пытаемся установить новую версию
				long originalVersion = Interlocked.CompareExchange(
					ref _lastProcessingDataVersion,
					newState.Version,
					currentVersion);

				// Если удалось установить - запускаем обработку
				if (originalVersion == currentVersion)
				{
					Task.Run(() => Rebuild(newState));
				}
			}
		};
	}

	public IUserAccessCacheState State => _state;

	public event EventHandler<IUserAccessCacheState>? StateChanged;

	private void Rebuild(IInventoryCacheState newDataState)
	{
		try
		{
			// Вычисляем новые права доступа
			var newState = Measures.Measure(() => CreateDerivedState(newDataState), _logger, $"{nameof(UserAccessCacheStore)}");

			// Атомарно обновляем состояние
			Interlocked.Exchange(ref _state, newState);

			// Убеждаемся, что мы обработали самую свежую версию
			long currentVersion = Interlocked.Read(ref _lastProcessingDataVersion);
			if (newDataState.Version >= currentVersion)
			{
				// Вызываем событие только для самой актуальной версии
				Task.Run(() => StateChanged?.Invoke(this, newState));
			}

			_logger.LogInformation("Завершено обновление зависимых данных для версии {Version}", newDataState.Version);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Не удалось обновить зависимые данные для версии {Version}", newDataState.Version);
		}
	}

	private static UserAccessState CreateDerivedState(IInventoryCacheState newDataState)
	{
		return UserAccessStateFactory.Create(newDataState);
	}
}