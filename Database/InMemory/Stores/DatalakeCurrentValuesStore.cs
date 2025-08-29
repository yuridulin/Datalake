using Datalake.Database.Attributes;
using Datalake.Database.Repositories;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory.Stores;

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

	public async Task ReloadValuesAsync()
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		var values = await Measures.Measure(() => LoadValuesFromDatabaseAsync(db), _logger, nameof(LoadValuesFromDatabaseAsync));
		var newValues = new ConcurrentDictionary<int, TagHistory>(values);

		Interlocked.Exchange(ref _currentValues, newValues);

		await db.InsertAsync(new Log
		{
			Category = LogCategory.Core,
			Type = LogType.Success,
			Text = "Состояние текущих значений перезагружено",
		});

		_logger.LogInformation("Завершено обновление текущих значений");
	}

	public TagHistory? Get(int id) => _currentValues.TryGetValue(id, out var value) ? value : null;

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
				if (IsNew(existingValue, incomingValue))
					return incomingValue;

				updated = false;
				return existingValue;
			});

		return updated;
	}

	public bool IsNew(int id, TagHistory incoming)
	{
		var existing = Get(id);
		if (existing == null)
			return true;

		return IsNew(existing, incoming);
	}

	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<DatalakeDataStore> _logger;
	private ConcurrentDictionary<int, TagHistory> _currentValues = [];

	private static async Task<Dictionary<int, TagHistory>> LoadValuesFromDatabaseAsync(DatalakeContext db)
	{
		var dbValues = await ValuesRepository.ProtectedGetAllLastValuesAsync(db);
		var newValues = dbValues.ToDictionary(x => x.TagId);
		return newValues;
	}

	private static bool IsNew(TagHistory existing, TagHistory incoming)
	{
		if (incoming.Date < existing.Date)
			return false; // запись в прошлое
		else
		{
			if (!AreAlmostEqual(incoming.Number, existing.Number) || incoming.Text != existing.Text || incoming.Quality != existing.Quality)
				return true; // значения не совпадают

			return false; // значения совпали, значит повтор
		}
	}

	private static bool AreAlmostEqual(float? value1, float? value2, double epsilon = 0.00001)
	{
		var rounded = Math.Abs((value1 ?? 0) - (value2 ?? 0));
		return rounded < epsilon;
	}
}

#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена