using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Datalake.Data.Infrastructure.InMemory;

[Singleton]
public class CurrentValuesStore(
	ILogger<CurrentValuesStore> logger) : ICurrentValuesStore
{
	private ConcurrentDictionary<int, TagHistoryValue> currentValues = [];
	private readonly SemaphoreSlim semaphore = new(1, 1);

	public async Task ReloadValuesAsync(IEnumerable<TagHistoryValue> values)
	{
		await semaphore.WaitAsync();

		try
		{

			var valuesDict = values.ToDictionary(x => x.TagId);
			var newValues = new ConcurrentDictionary<int, TagHistoryValue>(valuesDict);

			Interlocked.Exchange(ref currentValues, newValues);

			logger.LogInformation("Завершено обновление текущих значений");
		}
		finally
		{
			semaphore.Release();
		}
	}

	public TagHistoryValue? TryGet(int id)
	{
		return currentValues.TryGetValue(id, out var value) ? value : null;
	}

	public Dictionary<int, TagHistoryValue?> GetByIdentifiers(int[] identifiers)
	{
		var state = currentValues;

		Dictionary<int, TagHistoryValue?> result = [];
		foreach (var id in identifiers)
		{
			if (result.ContainsKey(id)) // если один тег запрошен несколько раз за один запрос
				continue;

			state.TryGetValue(id, out var value);
			result.Add(id, value);
		}

		return result;
	}

	public bool TryUpdate(int tagId, TagHistoryValue incomingValue)
	{
		bool updated = true;

		currentValues.AddOrUpdate(
			tagId,
			incomingValue,
			(key, existingValue) =>
			{
				if (existingValue.IsNew(incomingValue))
					return incomingValue;

				updated = false;
				return existingValue;
			});

		return updated;
	}

	public bool IsNew(int id, TagHistoryValue incomingValue)
	{
		var existingValue = TryGet(id);
		if (existingValue == null)
			return true;

		return existingValue.IsNew(incomingValue);
	}
}

