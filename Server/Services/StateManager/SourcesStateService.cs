using Datalake.Database.Repositories;
using Datalake.PublicApi.Constants;
using Datalake.Server.Models.System;

namespace Datalake.Server.Services.StateManager;

/// <summary>
/// Кэш состояний источников данных
/// </summary>
public class SourcesStateService
{
	object locker = new();

	/// <summary>
	/// Текущие состояния источников данных
	/// </summary>
	public Dictionary<int, SourceState> State { get; set; } = [];

	/// <summary>
	/// Инициализация списка информации
	/// </summary>
	/// <param name="sourcesId">Идентификаторы источников</param>
	public void Initialize(int[] sourcesId)
	{
		var now = DateFormats.GetCurrentDateTime();
		lock (locker)
		{
			State.Clear();

			foreach (var sourceId in sourcesId)
			{
				var state = new SourceState
				{
					SourceId = sourceId,
					IsTryConnected = false,
				};

				UpdateValuesInfo(state, now);

				State[sourceId] = state;
			}
		}
	}

	/// <summary>
	/// Обновление информации о состоянии источника данных
	/// </summary>
	public void UpdateSource(int sourceId, bool connected = false)
	{
		var now = DateFormats.GetCurrentDateTime();
		lock (locker)
		{
			var value = State.TryGetValue(sourceId, out var state);
			if (state != null)
			{
				state.LastTry = now;
				state.IsConnected = connected;
			}
			else
			{
				state = new SourceState
				{
					SourceId = sourceId,
					LastTry = now,
					IsConnected = connected
				};
				State[sourceId] = state;
			}

			if (!state.IsTryConnected) state.IsTryConnected = true;
			if (connected)
			{
				state.LastConnection = now;
			}

			UpdateValuesInfo(state, now);
		}
	}

	private static void UpdateValuesInfo(SourceState sourceState, DateTime now)
	{
		var tags = ValuesRepository.GetLiveValues(sourceState.SourceId);
		sourceState.ValuesAfterWriteSeconds = tags.Select(x => (int)(now - x.Date).TotalSeconds).ToArray();
	}
}
