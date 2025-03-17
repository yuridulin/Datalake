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

			if (connected)
			{
				state.LastConnection = now;
			}
		}
	}
}
