using Datalake.DataService.Database.Interfaces;
using Datalake.DataService.Services.Collection.Abstractions;
using Datalake.DataService.Services.Metrics;
using Datalake.Inventory.Models;
using Datalake.PrivateApi.Attributes;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Values;

namespace Datalake.DataService.Services.Collection.Internal;

[Transient]
public class AggregateCollector : CollectorBase
{
	public AggregateCollector(
		IServiceScopeFactory serviceScopeFactory,
		TagsStateService tagsStateService,
		SourceWithTagsInfo source,
		SourcesStateService sourcesStateService,
		TagsReceiveStateService receiveStateService,
		ILogger<AggregateCollector> logger) : base("Агрегатные значения", source, sourcesStateService, logger, 100)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_tagsStateService = tagsStateService;
		_receiveStateService = receiveStateService;

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

		_minuteRules = _allRules.Where(x => x.Period == AggregationPeriod.Minute).ToArray();
		_hourRules = _allRules.Where(x => x.Period == AggregationPeriod.Hour).ToArray();
		_dayRules = _allRules.Where(x => x.Period == AggregationPeriod.Day).ToArray();
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
	private readonly TagsStateService _tagsStateService;
	private readonly TagsReceiveStateService _receiveStateService;
	private int _lastMinute = -1;
	private int _lastHour = -1;
	private int _lastDay = -1;

	protected override async Task Work()
	{
		var now = DateFormats.GetCurrentDateTime();

		var minute = now.Minute;
		var hour = now.Hour;
		var day = now.Day;

		List<ValueWriteRequest> records = [];

		if (_minuteRules.Length > 0 && _lastMinute != minute)
		{
			_logger.LogInformation("Расчет минутных значений: {now}", now);
			var minuteValues = await GetValuesAsync(_minuteRules, now, AggregationPeriod.Minute);
			records.AddRange(minuteValues);
			_lastMinute = minute;
		}

		if (_hourRules.Length > 0 && _lastHour != hour && !_tokenSource.IsCancellationRequested)
		{
			_logger.LogInformation("Расчет часовых значений: {now}", now);
			var hourValues = await GetValuesAsync(_hourRules, now, AggregationPeriod.Hour);
			records.AddRange(hourValues);
			_lastHour = hour;
		}

		if (_dayRules.Length > 0 && _lastDay != day && !_tokenSource.IsCancellationRequested)
		{
			_logger.LogInformation("Расчет суточных значений: {now}", now);
			var dayValues = await GetValuesAsync(_dayRules, now, AggregationPeriod.Day);
			records.AddRange(dayValues);
			_lastDay = day;
		}

		if (records.Count > 0)
			await WriteAsync(records);
	}

	private async Task<List<ValueWriteRequest>> GetValuesAsync(TagAggregationRule[] rules, DateTime date, AggregationPeriod period)
	{
		TagAggregationWeightedValue[] aggregated = await GetAggregatedValuesAsync(rules, date, period);

		_tagsStateService.UpdateTagState([
			new()
			{
				RequestKey = "aggregate-collector-" + period switch
				{
					AggregationPeriod.Minute => "min",
					AggregationPeriod.Hour => "hour",
					AggregationPeriod.Day => "day",
					_ => throw new NotImplementedException("Этот тип агрегации не поддерживается")
				},
				TagsId = rules.Select(x => x.TagSourceId).ToArray()
			}
		]);

		var result = aggregated
			.Select(value => new
			{
				Value = value,
				Tag = rules.FirstOrDefault(x => x.TagSourceId == value.TagId),
			})
			.Where(x => x.Tag != null)
			.Select(x => new ValueWriteRequest
			{
				Date = x.Value.Date,
				Guid = x.Tag!.Guid,
				Id = x.Tag.Id,
				Name = x.Tag.Name,
				Quality = TagQuality.Good,
				Value = x.Tag.Type switch
				{
					TagAggregation.Sum => x.Value.Sum,
					TagAggregation.Average => x.Value.Average,
					_ => null,
				},
			})
			.ToList();

		foreach (var x in result)
		{
			if (x.Id.HasValue)
				_receiveStateService.Set(x.Id.Value, x.Value == null ? "Значение не получено" : null);
		}

		return result;
	}

	private async Task<TagAggregationWeightedValue[]> GetAggregatedValuesAsync(
		TagAggregationRule[] rules,
		DateTime date,
		AggregationPeriod period)
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

		public required AggregationPeriod Period { get; set; }
	}

	#endregion
}
