using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
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
		SourceSettingsDto source,
		ILogger<AggregateCollector> logger) : base(source, logger, 100)
	{
		_serviceScopeFactory = serviceScopeFactory;

		_allRules = source.NotDeletedTags
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

		_minuteRules = _allRules.Where(x => x.AggregatePeriod == TagResolution.Minute).ToArray();
		_hourRules = _allRules.Where(x => x.AggregatePeriod == TagResolution.Hour).ToArray();
		_dayRules = _allRules.Where(x => x.AggregatePeriod == TagResolution.Day).ToArray();
	}

	public override void Start(CancellationToken stoppingToken)
	{
		if (_allRules.Length == 0)
		{
			Task.Run(() => WriteAsync([], false), stoppingToken);
			_logger.LogWarning("Сборщик \"{name}\" не имеет правил агрегирования и не будет запущен", _name);
			return;
		}

		Task.Run(() => WriteAsync([], true), stoppingToken);
		base.Start(stoppingToken);
	}

	#region Реализация

	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly TagAggregationRule[] _allRules;
	private readonly TagAggregationRule[] _minuteRules;
	private readonly TagAggregationRule[] _hourRules;
	private readonly TagAggregationRule[] _dayRules;
	private int _lastMinute = -1;
	private int _lastHour = -1;
	private int _lastDay = -1;

	protected override async Task Work()
	{
		var now = DateTimeExtension.GetCurrentDateTime();

		var minute = now.Minute;
		var hour = now.Hour;
		var day = now.Day;

		List<TagValue> records = [];

		if (_minuteRules.Length > 0 && _lastMinute != minute)
		{
			_logger.LogInformation("Расчет минутных значений: {now}", now);
			var minuteValues = await GetValuesAsync(_minuteRules, now, TagResolution.Minute);
			records.AddRange(minuteValues);
			_lastMinute = minute;
		}

		if (_hourRules.Length > 0 && _lastHour != hour && !_tokenSource.IsCancellationRequested)
		{
			_logger.LogInformation("Расчет часовых значений: {now}", now);
			var hourValues = await GetValuesAsync(_hourRules, now, TagResolution.Hour);
			records.AddRange(hourValues);
			_lastHour = hour;
		}

		if (_dayRules.Length > 0 && _lastDay != day && !_tokenSource.IsCancellationRequested)
		{
			_logger.LogInformation("Расчет суточных значений: {now}", now);
			var dayValues = await GetValuesAsync(_dayRules, now, TagResolution.Day);
			records.AddRange(dayValues);
			_lastDay = day;
		}

		if (records.Count > 0)
			await WriteAsync(records);
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
		using var scope = _serviceScopeFactory.CreateScope();
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
