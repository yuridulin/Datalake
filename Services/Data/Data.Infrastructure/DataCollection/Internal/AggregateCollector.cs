using Datalake.Contracts.Public.Enums;
using Datalake.Data.Api.Models;
using Datalake.Data.Application.DataCollection.Models;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Domain.Extensions;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.Internal;

[Transient]
public class AggregateCollector : CollectorBase
{
	public AggregateCollector(
		IServiceScopeFactory serviceScopeFactory,
		SourceSettingsDto source,
		ILogger<AggregateCollector> logger) : base(source, logger, 100)
	{
		_serviceScopeFactory = serviceScopeFactory;

		_allRules = source.Tags
			.Select(tag => new TagAggregationRule
			{
				Id = tag.Id,
				TagSourceId = tag.SourceTag?.InputTagId ?? 0,
				Period = tag.AggregationPeriod ?? 0,
				Type = tag.Aggregation ?? 0,
				Guid = tag.Guid,
				Name = tag.Name,
			})
			.Where(rule => rule.TagSourceId != 0 && rule.Period != 0 && rule.Type != 0)
			.ToArray();

		_minuteRules = _allRules.Where(x => x.Period == TagResolution.Minute).ToArray();
		_hourRules = _allRules.Where(x => x.Period == TagResolution.Hour).ToArray();
		_dayRules = _allRules.Where(x => x.Period == TagResolution.Day).ToArray();
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

		List<TagHistory> records = [];

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

	private async Task<List<TagHistory>> GetValuesAsync(TagAggregationRule[] rules, DateTime date, TagResolution period)
	{
		TagAggregationWeightedValue[] aggregated = await GetAggregatedValuesAsync(rules, date, period);

		var result = aggregated
			.Select(value => new
			{
				Value = value,
				Tag = rules.FirstOrDefault(x => x.TagSourceId == value.TagId),
			})
			.Where(x => x.Tag != null)
			.Select(x => x.Tag.Type switch
			{
				TagAggregation.Sum => new TagHistory(x.Tag.Id, x.Value.Date, TagQuality.Good, x.Value.Sum, 1),
				TagAggregation.Average =>new TagHistory(x.Tag.Id, x.Value.Date, TagQuality.Good, x.Value.Average, 1),
				_ => null,
			})
			.ToList();

		return result;
	}

	private async Task<TagAggregationWeightedValue[]> GetAggregatedValuesAsync(
		TagAggregationRule[] rules,
		DateTime date,
		TagResolution period)
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var aggregateRepository = scope.ServiceProvider.GetRequiredService<IGetAggregatedHistoryRepository>();

		var aggregated = await aggregateRepository.GetWeightedAggregatedValuesAsync(rules.Select(x => x.TagSourceId).ToArray(), date, period);
		return aggregated;
	}

	class TagAggregationRule
	{
		public required int Id { get; set; }

		public required Guid Guid { get; set; }

		public required string Name { get; set; }

		public required int TagSourceId { get; set; }

		public required TagAggregation Type { get; set; }

		public required TagResolution Period { get; set; }
	}

	#endregion
}
