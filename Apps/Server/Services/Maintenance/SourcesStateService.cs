using Datalake.Database.InMemory.Stores;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Sources;
using System.Collections.Concurrent;

namespace Datalake.Server.Services.Maintenance;

/// <summary>
/// Кэш состояний источников данных
/// </summary>
public class SourcesStateService(
	DatalakeDataStore dataStore,
	DatalakeCurrentValuesStore valuesStore)
{
	private ConcurrentDictionary<int, SourceStateInfo> _state = [];

	/// <summary>
	/// Получение текущего состояния источников данных
	/// </summary>
	public Dictionary<int, SourceStateInfo> State() => new(_state);

	/// <summary>
	/// Инициализация списка информации
	/// </summary>
	/// <param name="sourcesId">Идентификаторы источников</param>
	public void Initialize(int[] sourcesId)
	{
		var newState = new ConcurrentDictionary<int, SourceStateInfo>();

		foreach (var sourceId in sourcesId)
			UpdateSource(sourceId, false);

		Interlocked.Exchange(ref _state, newState);
	}

	/// <summary>
	/// Обновление информации о состоянии источника данных
	/// </summary>
	public void UpdateSource(int sourceId, bool connected = false)
	{
		var now = DateFormats.GetCurrentDateTime();

		var tags = dataStore.State.Tags;
		int allCount = 0;
		int lastHalfHour = 0;
		int lastDay = 0;

		foreach (var tag in tags)
		{
			if (tag.SourceId != sourceId)
				continue;

			allCount++;

			var history = valuesStore.Get(tag.Id);
			if (history == null)
				continue;

			var lastUpdate = now - history.Date;

			if (lastUpdate <= HalfHour)
			{
				lastHalfHour++;
				lastDay++;
			}
			else if (lastUpdate <= Day)
			{
				lastDay++;
			}
		}

		_state.AddOrUpdate(
			sourceId,
			(id) => new SourceStateInfo
			{
				SourceId = id,
				LastTry = now,
				IsTryConnected = true,
				LastConnection = now,
				IsConnected = connected,
				ValuesAll = allCount,
				ValuesLastHalfHour = lastHalfHour,
				ValuesLastDay = lastDay,
			},
			(id, state) => new SourceStateInfo
			{
				SourceId = id,
				LastTry = now,
				IsTryConnected = true,
				LastConnection = now,
				IsConnected = connected,
				ValuesAll = allCount,
				ValuesLastHalfHour = lastHalfHour,
				ValuesLastDay = lastDay,
			});
	}

	static readonly TimeSpan HalfHour = TimeSpan.FromMinutes(30);
	static readonly TimeSpan Day = TimeSpan.FromDays(1);
}
