using Datalake.InventoryService.InMemory.Models;
using Datalake.InventoryService.Services;
using Datalake.PrivateApi.Utils;

namespace Datalake.InventoryService.InMemory.Stores;

/// <summary>
/// Хранилище рассчитанных прав доступа данных
/// </summary>
public class DatalakeAccessStore
{
	private readonly ILogger<DatalakeAccessStore> _logger;
	private readonly AccessResolverService _accessResolverService;
	private readonly Lock _lock = new();
	private DatalakeAccessState _state;
	private long _lastProcessingDataVersion;

	public DatalakeAccessStore(
		DatalakeDataStore dataStore,
		AccessResolverService accessResolverService,
		ILogger<DatalakeAccessStore> logger)
	{
		_logger = logger;
		_accessResolverService = accessResolverService;

		// Вычисляем изначальное состояние сразу же
		lock (_lock)
		{
			_lastProcessingDataVersion = dataStore.State.Version;
			_state = Measures.Measure(() => CreateDerivedState(dataStore.State), _logger, $"{nameof(DatalakeAccessStore)}");
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

	/// <summary>
	/// Текущее состояние
	/// </summary>
	public DatalakeAccessState State => _state;

	/// <summary>
	/// Событие при изменении состояния
	/// </summary>
	public event EventHandler<DatalakeAccessState>? StateChanged;

	private void Rebuild(DatalakeDataState newDataState)
	{
		try
		{
			// Вычисляем новые права доступа
			var newState = Measures.Measure(() => CreateDerivedState(newDataState), _logger, $"{nameof(DatalakeAccessStore)}");

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

	private DatalakeAccessState CreateDerivedState(DatalakeDataState newDataState)
	{
		return _accessResolverService.Resolve(newDataState);
	}
}