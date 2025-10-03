using Datalake.Data.Host.Abstractions;
using Datalake.Data.Host.Database.Entities;
using Datalake.Data.Host.Database.Interfaces;
using Datalake.Shared.Application;
using System.Collections.Concurrent;

namespace Datalake.Data.Host.Stores;

[Singleton]
public class CurrentValuesStore : ICurrentValuesStore
{
	private readonly IServiceScopeFactory serviceScopeFactory;
	private readonly ILogger<CurrentValuesStore> logger;
	private ConcurrentDictionary<int, TagHistory> _currentValues = [];

	public CurrentValuesStore(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<CurrentValuesStore> logger)
	{
		this.serviceScopeFactory = serviceScopeFactory;
		this.logger = logger;

		// Загрузка текущих данных
		ReloadValuesAsync().Wait();
	}

	public async Task ReloadValuesAsync()
	{
		using var scope = serviceScopeFactory.CreateScope();
		var readerRepository = scope.ServiceProvider.GetRequiredService<IGetHistoryRepository>();

		var values = await readerRepository.GetLastValuesAsync();
		var valuesDict = values.ToDictionary(x => x.TagId);
		var newValues = new ConcurrentDictionary<int, TagHistory>(valuesDict);

		Interlocked.Exchange(ref _currentValues, newValues);

		logger.LogInformation("Завершено обновление текущих значений");
	}

	public TagHistory? TryGet(int id) => _currentValues.TryGetValue(id, out var value) ? value : null;

	public Dictionary<int, TagHistory?> GetByIdentifiers(int[] identifiers)
	{
		var state = _currentValues;

		Dictionary<int, TagHistory?> result = [];
		foreach (var id in identifiers)
		{
			if (result.ContainsKey(id)) // если один тег запрошен несколько раз за один запрос
				continue;

			state.TryGetValue(id, out var value);
			result.Add(id, value);
		}

		return result;
	}

	public bool TryUpdate(int id, TagHistory incomingValue)
	{
		bool updated = true;

		_currentValues.AddOrUpdate(
			id,
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

	public bool IsNew(int id, TagHistory incomingValue)
	{
		var existingValue = TryGet(id);
		if (existingValue == null)
			return true;

		return existingValue.IsNew(incomingValue);
	}
}