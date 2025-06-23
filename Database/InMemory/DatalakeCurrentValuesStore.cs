using Datalake.Database.InMemory.Repositories;
using Datalake.Database.Tables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Datalake.Database.InMemory;

#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена

public class DatalakeCurrentValuesStore
{
	public DatalakeCurrentValuesStore(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<DatalakeDataStore> logger)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;

		_ = LoadValuesFromDatabaseAsync();
	}

	public async Task LoadValuesFromDatabaseAsync()
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();
		var valuesRepository = scope.ServiceProvider.GetRequiredService<ValuesRepository>();

		var t = Stopwatch.StartNew();

		var dbValues = await ValuesRepository.ReadLastValuesAsync(db);
		var newValues = new ConcurrentDictionary<int, TagHistory>(dbValues);

		Interlocked.Exchange(ref _currentValues, newValues);

		t.Stop();
		_logger.LogInformation("Загрузка БД: {ms}", t.Elapsed.TotalMilliseconds);
	}

	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<DatalakeDataStore> _logger;
	private ConcurrentDictionary<int, TagHistory> _currentValues = [];

	public TagHistory? Get(int id) => _currentValues.TryGetValue(id, out var value) ? value : null;

	public bool TryUpdate(int id, TagHistory incomingValue)
	{
		bool updated = false;

		_currentValues.AddOrUpdate(
			id,
			incomingValue,
			(key, existingValue) =>
			{
				if (incomingValue.Date > existingValue.Date && (
					incomingValue.Number != existingValue.Number ||
					incomingValue.Text != existingValue.Text ||
					incomingValue.Quality != existingValue.Quality))
				{
					updated = true;
					return incomingValue;
				}
				return existingValue;
			});

		return updated;
	}
}

#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
