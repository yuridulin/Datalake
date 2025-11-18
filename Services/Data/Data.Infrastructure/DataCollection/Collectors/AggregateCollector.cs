using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Application.Models.Tags;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Data.Infrastructure.DataCollection.Interfaces;
using Datalake.Data.Infrastructure.DataCollection.Models;
using Datalake.Domain.Entities;
using Datalake.Domain.Enums;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.Collectors;

[Transient]
public class AggregateCollector(
	ISourcesActivityStore sourcesActivityStore,
	IServiceScopeFactory serviceScopeFactory,
	IDataCollectorWriter writer,
	ILogger<AggregateCollector> _,
	SourceSettingsDto source) : DataCollectorBase(sourcesActivityStore, writer, _, source)
{
	public override Task StartAsync(CancellationToken cancellationToken = default)
	{
		bool notEmpty = false;

		foreach (var tag in source.NotDeletedTags)
		{
			if (tag.AggregationSettings == null)
				continue;

			if (!notEmpty)
				notEmpty = true;
			var rule = new TagAggregationRule
			{
				TagId = tag.TagId,
				AggregateFunction = tag.AggregationSettings!.AggregateFunction,
				AggregatePeriod = tag.AggregationSettings.AggregatePeriod,
				SourceTagId = tag.AggregationSettings.SourceTagId,
				SourceTagType = tag.AggregationSettings.SourceTagType,
			};

			if (tag.AggregationSettings.AggregatePeriod == TagResolution.Minute)
			{
				minuteRules[rule.SourceTagId] = rule;
			}
			else if (tag.AggregationSettings.AggregatePeriod == TagResolution.Hour)
			{
				hourRules[rule.SourceTagId] = rule;
			}
			else if (tag.AggregationSettings.AggregatePeriod == TagResolution.Day)
			{
				dayRules[rule.SourceTagId] = rule;
			}
		}

		if (!notEmpty)
			return NotStartAsync("нет правил агрегации");

		return base.StartAsync(cancellationToken);
	}

	protected override async Task ExecuteAsync(CollectorUpdate state, CancellationToken cancellationToken)
	{
		var now = DateTime.UtcNow;

		var minute = now.Minute;
		var hour = now.Hour;
		var day = now.Day;

		if (minuteRules.Count > 0 && lastMinute != minute)
		{
			if (logger.IsEnabled(LogLevel.Information))
				logger.LogInformation("Расчет минутных значений: {now}", now);

			var minuteValues = await GetValuesAsync(minuteRules, now, TagResolution.Minute, cancellationToken);
			state.Values.AddRange(minuteValues);
			lastMinute = minute;
		}

		if (hourRules.Count > 0 && lastHour != hour)
		{
			if (logger.IsEnabled(LogLevel.Information))
				logger.LogInformation("Расчет часовых значений: {now}", now);

			var hourValues = await GetValuesAsync(hourRules, now, TagResolution.Hour, cancellationToken);
			state.Values.AddRange(hourValues);
			lastHour = hour;
		}

		if (dayRules.Count > 0 && lastDay != day)
		{
			if (logger.IsEnabled(LogLevel.Information))
				logger.LogInformation("Расчет суточных значений: {now}", now);

			var dayValues = await GetValuesAsync(dayRules, now, TagResolution.Day, cancellationToken);
			state.Values.AddRange(dayValues);
			lastDay = day;
		}

		state.IsActive = true;
	}

	private async Task<List<TagValue>> GetValuesAsync(Dictionary<int, TagAggregationRule> rules, DateTime date, TagResolution period, CancellationToken cancellationToken)
	{
		TagWeightedValue[] aggregated = await GetAggregatedValuesAsync(rules.Keys, date, period, cancellationToken);

		var result = aggregated
			.Select(value => new
			{
				Value = value,
				Tag = rules.TryGetValue(value.TagId, out var tag) ? tag : null,
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
		IReadOnlyCollection<int> identifiers,
		DateTime date,
		TagResolution period,
		CancellationToken cancellationToken)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var aggregationRepository = scope.ServiceProvider.GetRequiredService<ITagsValuesAggregationRepository>();

		var aggregatedValues = await aggregationRepository.GetWeightedValuesAsync(identifiers, date, period, cancellationToken);
		return aggregatedValues;
	}

	private Dictionary<int, TagAggregationRule> minuteRules = [];
	private Dictionary<int, TagAggregationRule> hourRules = [];
	private Dictionary<int, TagAggregationRule> dayRules = [];
	private int lastMinute = -1;
	private int lastHour = -1;
	private int lastDay = -1;

	record TagAggregationRule : TagAggregationSettingsDto
	{
		public int TagId { get; init; }
	}
}
