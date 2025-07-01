using Datalake.Database.Attributes;
using Datalake.Database.Repositories;
using Datalake.Database.Tables;
using Datalake.PublicApi.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

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

		_ = Measures.Measure(ReloadValuesAsync, _logger, nameof(ReloadValuesAsync));
	}

	private async Task ReloadValuesAsync()
	{
		var values = await Measures.Measure(LoadValuesFromDatabaseAsync, _logger, nameof(LoadValuesFromDatabaseAsync));
		var newValues = new ConcurrentDictionary<int, TagHistory>(values);

		Interlocked.Exchange(ref _currentValues, newValues);

		_logger.LogInformation("Завершено обновление текущих значений");
	}

	private async Task<Dictionary<int, TagHistory>> LoadValuesFromDatabaseAsync()
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		var dbValues = await ValuesRepository.ProtectedReadAllLastValuesAsync(db);
		var newValues = dbValues.ToDictionary(x => x.TagId);
		return newValues;
	}

	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<DatalakeDataStore> _logger;
	private ConcurrentDictionary<int, TagHistory> _currentValues = [];

	public TagHistory? Get(int id) => _currentValues.TryGetValue(id, out var value) ? value : null;

	public Dictionary<int, TagHistory> GetByIdentifiers(int[] identifiers)
	{
		var state = _currentValues;
		var now = DateFormats.GetCurrentDateTime();

		Dictionary<int, TagHistory> result = [];
		foreach (var id in identifiers)
		{
			state.TryGetValue(id, out var value);
			result.Add(id, value ?? new()
			{
				TagId = id,
				Date = now,
				Text = null,
				Number = null,
				Quality = PublicApi.Enums.TagQuality.Bad_NoValues,
			});
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
				bool isIncomingNew = incomingValue.Date > existingValue.Date && (
					!AreAlmostEqual(incomingValue.Number, existingValue.Number) ||
					incomingValue.Text != existingValue.Text ||
					incomingValue.Quality != existingValue.Quality);

				if (!isIncomingNew)
				{
					updated = false;
					return existingValue;
				}

				return incomingValue;
			});

		return updated;
	}

	private static bool AreAlmostEqual(float? value1, float? value2, double epsilon = 0.00001)
	{
		var rounded = Math.Abs((value1 ?? 0) - (value2 ?? 0));
		return rounded < epsilon;
	}
}

#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
