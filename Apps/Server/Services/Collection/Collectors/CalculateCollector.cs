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
		if (_expressions.Length == 0)
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

		foreach (var (tag, map) in _thresholds)
		{
			var record = new ValueWriteRequest
			{
				Date = now,
				Guid = tag.Guid,
				Id = tag.Id,
				Name = tag.Name,
				Value = null,
				Quality = TagQuality.Bad_NoConnect,
			};

			var incomingValue = valuesStore.Get(tag.Id);

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
			}

			batch.Add(record);
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
	private (SourceTagInfo tag, (float inValue, float outValue)[] thresholds)[] _thresholds = source.Tags
		.Where(tag => 
			tag.Type == TagType.Number &&
			tag.Calculation == TagCalculation.Thresholds &&
			tag.Thresholds != null &&
			tag.Thresholds.Count > 0)
		.Select(tag => (
			tag,
			tag.Thresholds!
				.OrderBy(x => x.Threshold)
				.Select(x => (x.Threshold, x.Result))
				.ToArray()))
		.ToArray();

	/// <summary>
	/// Поиск значения по таблице порогов:
	/// наибольший порог ≤ входного, иначе крайний ближний.
	/// Массив table должен быть отсортирован по inValue по возрастанию.
	/// </summary>
	private static float LookupValue((float inValue, float outValue)[] table, float input)
	{
		// Ищем по первому элементу кортежа
		int idx = Array.BinarySearch(
			table,
			(input, 0f),
			ThresholdComparer
		);

		if (idx >= 0)
			return table[idx].outValue; // точное совпадение

		idx = ~idx - 1; // индекс "пола"

		if (idx < 0)
			return table[0].outValue; // всё больше входного — берём первый

		if (idx >= table.Length)
			return table[^1].outValue; // всё меньше входного — берём последний

		return table[idx].outValue;
	}

	private static readonly Comparer<(float inValue, float outValue)> ThresholdComparer =
		Comparer<(float inValue, float outValue)>.Create((a, b) => a.inValue.CompareTo(b.inValue));
}
