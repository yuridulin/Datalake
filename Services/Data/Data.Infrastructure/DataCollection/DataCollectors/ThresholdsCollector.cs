﻿using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Application.Models.Tags;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.DataCollectors;

[Transient]
public class ThresholdsCollector(
	ICurrentValuesStore valuesStore,
	IDataCollectionErrorsStore errorsStore,
	IDataCollectorProcessor processor,
	ILogger<DatalakeCollector> logger,
	SourceSettingsDto source) : DataCollectorBase(processor, logger, source)
{
	public override Task StartAsync(CancellationToken stoppingToken)
	{
		if (_thresholds.Length == 0)
		{
			this.logger.LogWarning("Сборщик \"{name}\" не имеет правил расчета и не будет запущен", Name);
			return Task.CompletedTask;
		}

		return base.StartAsync(stoppingToken);
	}

	protected override async Task WorkAsync(CancellationToken cancellationToken)
	{
		var now = DateTimeExtension.GetCurrentDateTime();
		List<TagValue> batch = [];

		foreach (var (tag, inputId, map) in _thresholds)
		{
			if (!isRunning)
				break;

			var incomingValue = valuesStore.TryGet(inputId);
			if (incomingValue != null)
			{
				if (incomingValue.Number != null)
				{
					var outputValue = LookupValue(map, incomingValue.Number.Value);

					errorsStore.Set(tag.TagId, null);
					batch.Add(TagValue.AsNumeric(tag.TagId, now, TagQuality.Good, outputValue, tag.ScaleSettings?.GetScale()));
				}
				else
				{
					errorsStore.Set(tag.TagId, "Значение входного тега - не число");
					batch.Add(TagValue.AsEmpty(tag.TagId, now, TagQuality.Bad_NoValues));
				}
			}
		}

		await WriteValuesAsync(batch, cancellationToken);
	}


	/// <summary>
	/// Таблицы пороговых значений
	/// </summary>
	private (TagSettingsDto tag, int inputTagId, (float inValue, float outValue)[] thresholds)[] _thresholds = source.NotDeletedTags
		.Where(tag =>
			tag.TagType == TagType.Number &&
			tag.ThresholdsSettings != null)
		.Select(tag => (
			tag,
			tag.ThresholdsSettings!.SourceTagId,
			tag.ThresholdsSettings.Thresholds
				.OrderBy(x => x.InputValue)
				.Select(x => (x.InputValue, x.OutputValue))
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
			throw new ArgumentException("Таблица пороговых значений пуста", nameof(table));

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
