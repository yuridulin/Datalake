using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Datalake.Data.Infrastructure.InMemory;

[Singleton]
public class CurrentValuesStore(
	ILogger<CurrentValuesStore> logger) : ICurrentValuesStore
{
	private ConcurrentDictionary<int, TagValue> currentValues = [];
	private readonly SemaphoreSlim semaphore = new(1, 1);

	public async Task ReloadValuesAsync(IEnumerable<TagValue> values)
	{
		await semaphore.WaitAsync();

		try
		{

			var valuesDict = values.ToDictionary(x => x.TagId);
			var newValues = new ConcurrentDictionary<int, TagValue>(valuesDict);

			Interlocked.Exchange(ref currentValues, newValues);

			logger.LogInformation("Завершено обновление текущих значений");
		}
		finally
		{
			semaphore.Release();
		}
	}

	public TagValue? TryGet(int id)
	{
		return currentValues.TryGetValue(id, out var value) ? value : null;
	}

	public Dictionary<int, TagValue?> GetByIdentifiers(int[] identifiers)
	{
		var state = currentValues;

		Dictionary<int, TagValue?> result = [];
		foreach (var id in identifiers)
		{
			if (result.ContainsKey(id)) // если один тег запрошен несколько раз за один запрос
				continue;

			state.TryGetValue(id, out var value);
			result.Add(id, value);
		}

		return result;
	}

	public bool TryUpdate(IReadOnlyList<TagValue> incomingValues)
	{
		List<int> updatedValues = [];
		foreach (var incomingValue in incomingValues)
		{
			if (TryUpdate(incomingValue))
			{
				updatedValues.Add(incomingValue.TagId);
			}
		}

		if (updatedValues.Count > 0)
		{
			OnValuesChanged(updatedValues);
			return true;
		}

		return false;
	}

	public bool IsNew(int id, TagValue incomingValue)
	{
		var existingValue = TryGet(id);
		if (existingValue == null)
			return true;

		return existingValue.IsNew(incomingValue);
	}

	/// <inheritdoc/>
	public event EventHandler<ValuesChangedEventArgs>? ValuesChanged;

	private bool TryUpdate(TagValue incomingValue)
	{
		bool updated = true;
		currentValues.AddOrUpdate(
			incomingValue.TagId,
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

	/// <summary>
	/// Вызов события изменения значений тегов
	/// </summary>
	/// <param name="changedTagIds">Идентификаторы измененных тегов</param>
	private void OnValuesChanged(IReadOnlyList<int> changedTagIds)
	{
		ValuesChanged?.Invoke(this, new ValuesChangedEventArgs(changedTagIds));
	}
}
