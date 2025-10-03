using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Infrastructure.Cache.UserAccess;

[Singleton]
public class UserAccessCache : IUserAccessCache
{
	private readonly ILogger<UserAccessCache> _logger;
	private readonly Lock _lock = new();
	private UserAccessState _state;
	private long _lastProcessingDataVersion;

	public UserAccessCache(
		IInventoryCache inventoryCache,
		ILogger<UserAccessCache> logger)
	{
		_logger = logger;

		// Вычисляем изначальное состояние сразу же
		lock (_lock)
		{
			_lastProcessingDataVersion = inventoryCache.State.Version;
			_state = Measures.Measure(() => CreateDerivedState(inventoryCache.State), _logger, $"{nameof(UserAccessCache)}");
			_logger.LogInformation("Инициализация кэша прав доступа для версии {Version}", _lastProcessingDataVersion);
		}

		// Подписываемся на будущие изменения
		inventoryCache.StateChanged += (_, newState) =>
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
			var newState = Measures.Measure(() => CreateDerivedState(newDataState), _logger, $"{nameof(UserAccessCache)}");

			// Атомарно обновляем состояние
			Interlocked.Exchange(ref _state, newState);

			// Убеждаемся, что мы обработали самую свежую версию
			long currentVersion = Interlocked.Read(ref _lastProcessingDataVersion);
			if (newDataState.Version >= currentVersion)
			{
				// Вызываем событие только для самой актуальной версии
				Task.Run(() => StateChanged?.Invoke(this, newState));
			}

			_logger.LogInformation("Завершено обновление кэша прав доступа для версии {Version}", newDataState.Version);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Не удалось обновить кэш прав доступа для версии {Version}", newDataState.Version);
		}
	}

	private static UserAccessState CreateDerivedState(IInventoryCacheState newDataState)
	{
		return UserAccessStateFactory.Create(newDataState);
	}
}