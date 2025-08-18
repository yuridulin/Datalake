using Datalake.Database.InMemory;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.Services.Collection.Abstractions;
using Datalake.Server.Services.Maintenance;
using NCalc;

namespace Datalake.Server.Services.Collection.Collectors;

internal class CalculateCollector(
	DatalakeCurrentValuesStore valuesStore,
	TagsStateService tagsStateService,
	SourceWithTagsInfo source,
	SourcesStateService sourcesStateService,
	ILogger<CalculateCollector> logger) : CollectorBase("Расчетные значения", source, sourcesStateService, logger)
{
	public override void Start(CancellationToken stoppingToken)
	{
		if ((_expressions.Length + _thresholds.Length) == 0)
		{
			Task.Run(() => WriteAsync([], false), stoppingToken);
			_logger.LogWarning("Сборщик \"{name}\" не имеет правил расчета и не будет запущен", _name);
			return;
		}

		Task.Run(() => WriteAsync([], true), stoppingToken);
		base.Start(stoppingToken);
	}

	public override void PrepareToStop()
	{
		base.PrepareToStop();

		_expressions = [];
		_thresholds = [];
	}

	protected override async Task Work()
	{
		var now = DateFormats.GetCurrentDateTime();
		List<ValueWriteRequest> batch = [];
		HashSet<int> usedTags = [];

		foreach (var (tag, expression) in _expressions)
		{
			if (_tokenSource.IsCancellationRequested)
				break;

			var record = new ValueWriteRequest
			{
				Date = now,
				Guid = tag.Guid,
				Id = tag.Id,
				Name = tag.Name,
				Value = null,
				Quality = TagQuality.Bad_NoConnect,
			};

			expression.Parameters.Clear();

			string? error = null;

			foreach (var input in tag.FormulaInputs)
			{
				var inputRecord = valuesStore.Get(input.InputTagId);

				if (inputRecord == null)
				{
					error = $"Не найден входной тег #{input.InputTagId}";
					break;
				}

				usedTags.Add(inputRecord.TagId);

				object? inputValue = input.InputTagType switch
				{
					TagType.String => inputRecord.Text,
					TagType.Number => inputRecord.Number,
					TagType.Boolean => inputRecord.Number.HasValue ? inputRecord.Number == 1 : null,
				};

				if (inputValue == null)
				{
					error = $"У входного тега #{input.InputTagId} нет значения";
					break;
				}

				expression.Parameters[input.VariableName] = inputValue;
			}

			if (error == null)
			{
				try
				{
					var result = expression.Evaluate();

					if (result == null)
					{
						error = "Итоговое значение не получено";
					}
					else
					{
						record.Value = tag.Type switch
						{
							TagType.Number => Convert.ToSingle(result),
							TagType.String => Convert.ToString(result),
							TagType.Boolean => Convert.ToString(result),
						};
						record.Quality = TagQuality.Good;
					}
				}
				catch (Exception ex)
				{
					error = ex.Message;
				}
			}

			if (error != null)
			{
				record.Value = null;
				record.Quality = TagQuality.Bad_CalcError;
			}

			batch.Add(record);
		}

		foreach (var (tag, inputId, map) in _thresholds)
		{
			if (_tokenSource.IsCancellationRequested)
				break;

			var record = new ValueWriteRequest
			{
				Date = now,
				Guid = tag.Guid,
				Id = tag.Id,
				Name = tag.Name,
				Value = null,
				Quality = TagQuality.Bad_NoConnect,
			};

			var incomingValue = valuesStore.Get(inputId);

			if (incomingValue != null)
			{
				if (incomingValue.Number != null)
				{
					record.Value = LookupValue(map, incomingValue.Number.Value);
					record.Quality = TagQuality.Good;
				}
				else
				{
					record.Quality = TagQuality.Bad_NoValues;
				}

				usedTags.Add(inputId);
			}

			if ((record.Value as float?) != valuesStore.Get(tag.Id)?.Number)
			{
				batch.Add(record);
			}
		}

		await WriteAsync(batch);

		foreach (var tagId in usedTags)
			tagsStateService.UpdateTagState(tagId, CollectorRequestKey);
	}

	const string CollectorRequestKey = "calculate-collector";

	/// <summary>
	/// Формулы вычисления
	/// </summary>
	private (SourceTagInfo tag, Expression expression)[] _expressions = source.Tags
		.Where(tag =>
			tag.Calculation == TagCalculation.Formula &&
			tag.Formula != null)
		.Select(tag => (tag, new Expression(tag.Formula)))
		.ToArray();

	/// <summary>
	/// Таблицы пороговых значений
	/// </summary>
	private (SourceTagInfo tag, int inputTagId, (float inValue, float outValue)[] thresholds)[] _thresholds = source.Tags
		.Where(tag =>
			tag.Type == TagType.Number &&
			tag.Calculation == TagCalculation.Thresholds &&
			tag.ThresholdSourceTag != null &&
			tag.Thresholds != null &&
			tag.Thresholds.Count > 0)
		.Select(tag => (
			tag,
			tag.ThresholdSourceTag!.InputTagId,
			tag.Thresholds!
				.OrderBy(x => x.Threshold)
				.Select(x => (x.Threshold, x.Result))
				.ToArray()))
		.ToArray();

	/// <summary>
	/// Поиск значения по таблице уставок:
	/// берём ближайший по inValue; при равенстве — нижний (меньший) порог.
	/// Массив table должен быть отсортирован по inValue по возрастанию.
	/// </summary>
	private static float LookupValue((float inValue, float outValue)[] table, float input)
	{
		if (table == null || table.Length == 0)
			throw new ArgumentException("Threshold table must be non-empty", nameof(table));

		// Ищем по первому элементу кортежа (inValue)
		int idx = Array.BinarySearch(
				table,
				(input, 0f),
				ThresholdComparer
		);

		// Точное совпадение — сразу возвращаем
		if (idx >= 0)
			return table[idx].outValue;

		// Индексы ближайших соседей
		int ceilIdx = ~idx;         // первый элемент > input
		int floorIdx = ceilIdx - 1; // последний элемент < input

		// Вне диапазона — берём крайние
		if (floorIdx < 0)
			return table[0].outValue;

		if (ceilIdx >= table.Length)
			return table[^1].outValue;

		// Сравниваем расстояния до ближайших
		float downDist = input - table[floorIdx].inValue;
		float upDist = table[ceilIdx].inValue - input;

		// При равенстве берём нижний (меньший) порог — стабильный тай-брейк
		if (upDist < downDist)
			return table[ceilIdx].outValue;

		return table[floorIdx].outValue;
	}

	private static readonly Comparer<(float inValue, float outValue)> ThresholdComparer =
			Comparer<(float inValue, float outValue)>.Create((a, b) => a.inValue.CompareTo(b.inValue));
}
