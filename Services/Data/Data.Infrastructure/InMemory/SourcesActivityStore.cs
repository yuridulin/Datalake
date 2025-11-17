using Datalake.Contracts.Models.Sources;
using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Domain.Extensions;
using Datalake.Shared.Application.Attributes;
using System.Collections.Concurrent;

namespace Datalake.Data.Infrastructure.InMemory;

[Singleton]
public class SourcesActivityStore : ISourcesActivityStore
{
	private readonly ConcurrentDictionary<int, SourceActivityInfo> activity = new();

	public void Set(int sourceId, int tagsCount, bool isActive, int receivedCount)
	{
		var now = DateTimeExtension.GetCurrentDateTime();

		activity.AddOrUpdate(
			sourceId,
			_ => new SourceActivityInfo
			{
				SourceId = sourceId,
				LastTry = now,
				LastConnection = isActive ? now : null,
				IsConnected = isActive,
				ValuesAll = tagsCount,
				ValuesLastConnection = receivedCount,
				ValuesLastHalfHour = receivedCount,
				ValuesLastDay = receivedCount
			},
			(_, existing) =>
			{
				// обновляем только то, что изменилось
				return existing with
				{
					LastTry = now,
					LastConnection = isActive ? now : existing.LastConnection,
					IsConnected = isActive,
					ValuesAll = tagsCount,
					ValuesLastConnection = receivedCount,
					// обновление статистики
					ValuesLastHalfHour = UpdateWindow(existing.ValuesLastHalfHour, receivedCount, existing.LastTry, now, TimeSpan.FromMinutes(30)),
					ValuesLastDay = UpdateWindow(existing.ValuesLastDay, receivedCount, existing.LastTry, now, TimeSpan.FromDays(1))
				};
			});
	}

	public SourceActivityInfo Get(int sourceId)
	{
		activity.TryGetValue(sourceId, out var info);
		return info ?? new SourceActivityInfo { SourceId = sourceId, LastTry = null, IsConnected = false };
	}

	private static int UpdateWindow(int oldValue, int receivedCount, DateTime? lastTry, DateTime now, TimeSpan window)
	{
		if (lastTry == null || (now - lastTry.Value) > window)
			return receivedCount; // сброс окна

		return oldValue + receivedCount; // накапливаем
	}
}
