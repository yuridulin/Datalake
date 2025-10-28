using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Application.Models.Tags;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Data.Infrastructure.DataCollection.Interfaces;
using Datalake.Data.Infrastructure.DataCollection.Models;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.DataCollectors;

[Transient]
public class AggregateCollector : DataCollectorBase
{
	public AggregateCollector(
		IServiceScopeFactory serviceScopeFactory,
		IDataCollectorProcessor processor,
		ILogger<AggregateCollector> logger,
		SourceSettingsDto source) : base(processor, logger, source, 100)
	{
		this.serviceScopeFactory = serviceScopeFactory;

		allRules = source.NotDeletedTags
			.Where(x => x.AggregationSettings != null)
			.Select(tag => new TagAggregationRule
			{
				TagId = tag.TagId,
				AggregateFunction = tag.AggregationSettings!.AggregateFunction,
				AggregatePeriod = tag.AggregationSettings.AggregatePeriod,
				SourceTagId = tag.AggregationSettings.SourceTagId,
				SourceTagType = tag.AggregationSettings.SourceTagType,
			})
			.ToArray();

		minuteRules = allRules.Where(x => x.AggregatePeriod == TagResolution.Minute).ToArray();
		hourRules = allRules.Where(x => x.AggregatePeriod == TagResolution.Hour).ToArray();
		dayRules = allRules.Where(x => x.AggregatePeriod == TagResolution.Day).ToArray();
	}

	public override Task StartAsync(CancellationToken stoppingToken = default)
	{
		if (allRules.Length == 0)
		{
			logger.LogWarning("Сборщик {name} не имеет правил агрегирования и не будет запущен", Name);
			return Task.CompletedTask;
		}

		return base.StartAsync(stoppingToken);
	}

	#region Реализация

	private readonly IServiceScopeFactory serviceScopeFactory;
	private readonly TagAggregationRule[] allRules;
	private readonly TagAggregationRule[] minuteRules;
	private readonly TagAggregationRule[] hourRules;
	private readonly TagAggregationRule[] dayRules;
	private int lastMinute = -1;
	private int lastHour = -1;
	private int lastDay = -1;

	protected override async Task WorkAsync(CancellationToken cancellationToken)
	{
		var now = DateTimeExtension.GetCurrentDateTime();

		var minute = now.Minute;
		var hour = now.Hour;
		var day = now.Day;

		List<TagValue> records = [];

		if (isRunning && minuteRules.Length > 0 && lastMinute != minute)
		{
			logger.LogInformation("Расчет минутных значений: {now}", now);
			var minuteValues = await GetValuesAsync(minuteRules, now, TagResolution.Minute);
			records.AddRange(minuteValues);
			lastMinute = minute;
		}

		if (isRunning && hourRules.Length > 0 && lastHour != hour)
		{
			logger.LogInformation("Расчет часовых значений: {now}", now);
			var hourValues = await GetValuesAsync(hourRules, now, TagResolution.Hour);
			records.AddRange(hourValues);
			lastHour = hour;
		}

		if (isRunning && dayRules.Length > 0 && lastDay != day)
		{
			logger.LogInformation("Расчет суточных значений: {now}", now);
			var dayValues = await GetValuesAsync(dayRules, now, TagResolution.Day);
			records.AddRange(dayValues);
			lastDay = day;
		}

		if (records.Count > 0)
			await WriteValuesAsync(records, cancellationToken);
	}

	private async Task<List<TagValue>> GetValuesAsync(TagAggregationRule[] rules, DateTime date, TagResolution period)
	{
		TagWeightedValue[] aggregated = await GetAggregatedValuesAsync(rules, date, period);

		var result = aggregated
			.Select(value => new
			{
				Value = value,
				Tag = rules.FirstOrDefault(x => x.SourceTagId == value.TagId),
			})
			.Where(x => x.Tag != null)
			.Select(x => x.Tag.AggregateFunction switch
			{
				TagAggregation.Sum => TagValue.AsNumeric(x.Tag.TagId, x.Value.Date, TagQuality.Good, x.Value.Sum, 1),
				TagAggregation.Average => TagValue.AsNumeric(x.Tag.TagId, x.Value.Date, TagQuality.Good, x.Value.Average, 1),
				_ => null,
			})
			.Where(x => x != null)
			.ToList();

		return result!;
	}

	private async Task<TagWeightedValue[]> GetAggregatedValuesAsync(
		TagAggregationRule[] rules,
		DateTime date,
		TagResolution period)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var aggregationRepository = scope.ServiceProvider.GetRequiredService<ITagsValuesAggregationRepository>();

		var aggregatedValues = await aggregationRepository.GetWeightedValuesAsync(rules.Select(x => x.SourceTagId).ToArray(), date, period);
		return aggregatedValues;
	}

	record TagAggregationRule : TagAggregationSettingsDto
	{
		public int TagId { get; init; }
	}

	#endregion
}
