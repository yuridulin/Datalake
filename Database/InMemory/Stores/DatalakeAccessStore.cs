using Datalake.Database.Attributes;
using Datalake.Database.Functions;
using Datalake.Database.InMemory.Models;
using Microsoft.Extensions.Logging;

namespace Datalake.Database.InMemory.Stores;

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
			if (newState.Version > Volatile.Read(ref _lastProcessingVersion))
			{
				Volatile.Write(ref _lastProcessingVersion, newState.Version);
				Task.Run(() => Rebuild(newState));
			}
		};

		if (Volatile.Read(ref _lastProcessingVersion) == -1)
			Task.Run(() => Rebuild(dataStore.State));
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

	private Lock _rebuildLock = new();
	private long _lastProcessingVersion = -1;
	private readonly ILogger<DatalakeAccessStore> _logger;
	private DatalakeAccessState _accessState = new();

	private void Rebuild(DatalakeDataState newState)
	{
		lock (_rebuildLock)
		{
			if (newState.Version <= _lastProcessingVersion)
				return;

			try
			{
				Measures.Measure(() => RebuildAccess(newState), _logger, nameof(RebuildAccess));

				_logger.LogInformation("Завершено обновление прав доступа");
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Не удалось обновить права доступа");
			}
		}
	}

	private void RebuildAccess(DatalakeDataState state)
	{
		var accessState = AccessFunctions.ComputeAccess(state);
		Interlocked.Exchange(ref _accessState, accessState);

		Task.Run(() => AccessChanged?.Invoke(this, accessState));
	}
}