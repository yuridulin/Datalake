using Datalake.Database.Attributes;
using Datalake.Database.InMemory.Models;
using Microsoft.Extensions.Logging;

namespace Datalake.Database.InMemory.Stores.Derived;

/// <summary>
/// Хранилище производных данных
/// </summary>
public class DatalakeCachedTagsStore
{
	/// <summary>Конструктор</summary>
	public DatalakeCachedTagsStore(
		DatalakeDataStore dataStore,
		ILogger<DatalakeCachedTagsStore> logger)
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

	private Lock _rebuildLock = new();

	private void Rebuild(DatalakeDataState newState)
	{
		lock (_rebuildLock)
		{
			if (newState.Version <= _lastProcessingVersion)
				return;

			try
			{
				Measures.Measure(() => RebuildCachedTags(newState), _logger, nameof(RebuildCachedTags));

				_logger.LogInformation("Завершено обновление кэша тегов");
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Не удалось обновить кэш тегов");
			}
		}
	}

	/// <summary>
	/// Текущее состояние кэша тегов для записи
	/// </summary>
	public DatalakeCachedTagsState State => _cachedTagsState;

	private long _lastProcessingVersion = -1;
	private readonly ILogger<DatalakeCachedTagsStore> _logger;
	private DatalakeCachedTagsState _cachedTagsState = new();

	private void RebuildCachedTags(DatalakeDataState state)
	{
		var nextCachedTags = new DatalakeCachedTagsState(state);
		Interlocked.Exchange(ref _cachedTagsState, nextCachedTags);
	}
}