using Datalake.Database.Attributes;
using Datalake.Database.Functions;
using Datalake.Database.InMemory.Models;
using Microsoft.Extensions.Logging;

namespace Datalake.Database.InMemory.Stores.Derived;

/// <summary>
/// Хранилище рассчитанных прав доступа данных
/// </summary>
public class DatalakeAccessStore
{
	/// <summary>Конструктор</summary>
	public DatalakeAccessStore(
		DatalakeDataStore dataStore,
		ILogger<DatalakeAccessStore> logger)
	{
		_logger = logger;

		dataStore.StateChanged += (_, newState) =>
		{
			// Атомарно проверяем и обновляем версию
			long currentVersion = Interlocked.Read(ref _lastProcessingVersion);
			if (newState.Version > currentVersion)
			{
				// Пытаемся установить новую версию
				long originalVersion = Interlocked.CompareExchange(
						ref _lastProcessingVersion,
						newState.Version,
						currentVersion);

				// Если удалось установить - запускаем обработку
				if (originalVersion == currentVersion)
				{
					Task.Run(() => Rebuild(newState));
				}
			}
		};

		if (Volatile.Read(ref _lastProcessingVersion) == -1)
		{
			Volatile.Write(ref _lastProcessingVersion, dataStore.State.Version);
			Task.Run(() => Rebuild(dataStore.State));
		}
	}

	/// <summary>
	/// Разрешения пользователей, рассчитанные на каждый объект системы
	/// </summary>
	/// <returns>Разрешения, сгруппированные по идентификатору пользователя</returns>
	public DatalakeAccessState State => _accessState;

	/// <summary>
	/// Событие при изменении разрешений пользователей
	/// </summary>
	public event EventHandler<DatalakeAccessState>? AccessChanged;

	private long _lastProcessingVersion = -1;
	private readonly ILogger<DatalakeAccessStore> _logger;
	private DatalakeAccessState _accessState = new();

	private void Rebuild(DatalakeDataState newState)
	{
		try
		{
			// Вычисляем новые права доступа
			var accessState = Measures.Measure(() => AccessFunctions.ComputeAccess(newState), _logger, nameof(RebuildAccess));

			// Атомарно обновляем состояние
			Interlocked.Exchange(ref _accessState, accessState);

			// Убеждаемся, что мы обработали самую свежую версию
			long currentVersion = Interlocked.Read(ref _lastProcessingVersion);
			if (newState.Version >= currentVersion)
			{
				// Вызываем событие только для самой актуальной версии
				Task.Run(() => AccessChanged?.Invoke(this, accessState));
			}

			_logger.LogInformation("Завершено обновление прав доступа для версии {Version}", newState.Version);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Не удалось обновить права доступа");
		}
	}

	private void RebuildAccess(DatalakeDataState state)
	{
		var accessState = AccessFunctions.ComputeAccess(state);
		Interlocked.Exchange(ref _accessState, accessState);

		Task.Run(() => AccessChanged?.Invoke(this, accessState));
	}
}