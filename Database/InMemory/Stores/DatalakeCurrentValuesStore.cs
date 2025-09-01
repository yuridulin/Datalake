using Datalake.Database.Repositories;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory.Stores;

/// <summary>
/// Хранилище текущих значений тегов
/// </summary>
public class DatalakeCurrentValuesStore
{
	/// <summary>Конструктор</summary>
	public DatalakeCurrentValuesStore(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<DatalakeDataStore> logger)
	{
		this.serviceScopeFactory = serviceScopeFactory;
		this.logger = logger;

		// Загрузка текущих данных
		ReloadValuesAsync().Wait();
	}

	/// <summary>
	/// Обновление текущих значений из БД
	/// </summary>
	public async Task ReloadValuesAsync()
	{
		using var scope = serviceScopeFactory.CreateScope();
		using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		var values = await DatabaseValues.ReadAllCurrentAsync(db, logger);
		var valuesDict = values.ToDictionary(x => x.TagId);
		var newValues = new ConcurrentDictionary<int, TagHistory>(valuesDict);

		Interlocked.Exchange(ref _currentValues, newValues);

		await db.InsertAsync(new Log
		{
			Category = LogCategory.Core,
			Type = LogType.Success,
			Text = "Состояние текущих значений перезагружено",
		});

		logger.LogInformation("Завершено обновление текущих значений");
	}

	/// <summary>
	/// Получение значения по идентификатору
	/// </summary>
	/// <param name="id">Локальный идентификатор тега</param>
	/// <returns>Значение, если существует</returns>
	public TagHistory? Get(int id) => _currentValues.TryGetValue(id, out var value) ? value : null;

	/// <summary>
	/// Получение значений по идентификаторам в виде словаря
	/// </summary>
	/// <param name="identifiers">Локальные идентификаторы тегов</param>
	/// <returns>Значения, если есть, сопоставленные с идентификаторами</returns>
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

	/// <summary>
	/// Попытка записи нового значения. В процессе проходит проверка на новизну. Если значение не новее, то записи не будет.
	/// </summary>
	/// <param name="id">Локальный идентификатор тега</param>
	/// <param name="incomingValue">Значение для записи</param>
	/// <returns>Флаг, является ли значение новым</returns>
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

	/// <summary>
	/// Проверка, является ли значение новым
	/// </summary>
	/// <param name="id">Локальный идентификатор тега</param>
	/// <param name="incomingValue">Значение для проверки</param>
	/// <returns></returns>
	public bool IsNew(int id, TagHistory incomingValue)
	{
		var existingValue = Get(id);
		if (existingValue == null)
			return true;

		return IsNew(existingValue, incomingValue);
	}

	private readonly IServiceScopeFactory serviceScopeFactory;
	private readonly ILogger<DatalakeDataStore> logger;
	private ConcurrentDictionary<int, TagHistory> _currentValues = [];

	private static bool IsNew(TagHistory existingValue, TagHistory incomingValue)
	{
		if (incomingValue.Date < existingValue.Date)
			return false; // запись в прошлое
		else
		{
			if (!AreAlmostEqual(incomingValue.Number, existingValue.Number) || incomingValue.Text != existingValue.Text || incomingValue.Quality != existingValue.Quality)
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