using Datalake.Database.InMemory;
using Datalake.PublicApi.Constants;
using Datalake.Server.Services.Maintenance.Models;
using System.Collections.Concurrent;

namespace Datalake.Server.Services.Maintenance;

/// <summary>
/// Кэш состояний источников данных
/// </summary>
public class SourcesStateService(
	DatalakeDataStore dataStore,
	DatalakeCurrentValuesStore valuesStore)
{
	private ConcurrentDictionary<int, SourceState> _state = [];

	/// <summary>
	/// Получение текущего состояния источников данных
	/// </summary>
	public Dictionary<int, SourceState> State() => new(_state);

	/// <summary>
	/// Инициализация списка информации
	/// </summary>
	/// <param name="sourcesId">Идентификаторы источников</param>
	public void Initialize(int[] sourcesId)
	{
		var now = DateFormats.GetCurrentDateTime();

		var newState = new ConcurrentDictionary<int, SourceState>(
			sourcesId.ToDictionary(id => id, id => new SourceState
			{
				SourceId = id,
				IsTryConnected = false,
				LastTry = null,
				IsConnected = false,
				LastConnection = null,
				ValuesAfterWriteSeconds = GetSecondsAfterLastUpdate(id, now),
			}));

		Interlocked.Exchange(ref _state, newState);
	}

	/// <summary>
	/// Обновление информации о состоянии источника данных
	/// </summary>
	public void UpdateSource(int sourceId, bool connected = false)
	{
		var now = DateFormats.GetCurrentDateTime();

		double[] secondsAfterLastUpdate = GetSecondsAfterLastUpdate(sourceId, now);

		_state.AddOrUpdate(
			sourceId,
			(id) => new SourceState
			{
				SourceId = id,
				LastTry = now,
				IsTryConnected = true,
				LastConnection = now,
				IsConnected = connected,
				ValuesAfterWriteSeconds = secondsAfterLastUpdate,
			},
			(id, state) => new SourceState
			{
				SourceId = id,
				LastTry = now,
				IsTryConnected = true,
				LastConnection = now,
				IsConnected = connected,
				ValuesAfterWriteSeconds = secondsAfterLastUpdate,
			});
	}

	private double[] GetSecondsAfterLastUpdate(int sourceId, DateTime now)
	{
		return dataStore.State.CachesTags
			.Where(tag => tag.SourceId == sourceId)
			.Select(tag => tag.Id)
			.Select(valuesStore.Get)
			.Where(history => history != null)
			.Select(history => (now - history!.Date).TotalSeconds)
			.ToArray();
	}
}
