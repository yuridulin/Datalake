using Datalake.Database;
using Datalake.Database.Extensions;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.BackgroundServices.Collector.Models;
using Datalake.Server.Services.StateManager;

namespace Datalake.Server.BackgroundServices.Collector.Collectors;

internal class AggregateCollector : CollectorBase
{
	public AggregateCollector(
		DatalakeContext db,
		SourceWithTagsInfo source,
		TagsStateService tagsStateService,
		ILogger<AggregateCollector> logger) : base("Агрегатные значения", source, logger)
	{
		_tokenSource = new CancellationTokenSource();
		_db = db;
		_tagsStateService = tagsStateService;

		_allRules = source.Tags
			.Select(tag => new TagAggregationRule
			{
				Id = tag.Id,
				TagSourceId = tag.SourceTag?.InputTagId ?? 0,
				TagSourceGuid = tag.SourceTag?.InputTagGuid ?? Guid.Empty,
				Period = tag.AggregationPeriod ?? 0,
				Type = tag.Aggregation ?? 0,
				Guid = tag.Guid,
				Name = tag.Name,
			})
			.Where(rule => rule.TagSourceId != 0 && rule.Period != 0 && rule.Type != 0)
			.ToArray();

		_minuteRules = _allRules.Where(x => x.Period == AggregationPeriod.Munite).ToArray();
		_hourRules = _allRules.Where(x => x.Period == AggregationPeriod.Hour).ToArray();
		_dayRules = _allRules.Where(x => x.Period == AggregationPeriod.Day).ToArray();
	}

	public override Task Start(CancellationToken stoppingToken)
	{
		if (_allRules.Length == 0)
			return Task.CompletedTask;

		Task.Run(Work, stoppingToken);

		return base.Start(stoppingToken);
	}

	public override Task Stop()
	{
		_tokenSource.Cancel();

		return base.Stop();
	}

	public override event CollectEvent? CollectValues;

	#region Реализация

	private readonly CancellationTokenSource _tokenSource;
	private readonly DatalakeContext _db;
	private readonly TagAggregationRule[] _allRules;
	private readonly TagAggregationRule[] _minuteRules;
	private readonly TagAggregationRule[] _hourRules;
	private readonly TagAggregationRule[] _dayRules;
	private readonly TagsStateService _tagsStateService;

	private async Task Work()
	{
		while (!_tokenSource.Token.IsCancellationRequested)
		{
			var currentAggregationMoment = DateFormats.GetCurrentDateTime();
			_logger.LogInformation("Расчет агрегированных: : {currentAggregationMoment}", currentAggregationMoment);

			try
			{
				bool isDay = currentAggregationMoment.Hour == 0;
				List<CollectValue> collectValues;

				collectValues = await GetAggregated(_minuteRules, currentAggregationMoment, AggregationPeriod.Munite);

				if (currentAggregationMoment.Minute == 0)
				{
					collectValues.AddRange(await GetAggregated(_hourRules, currentAggregationMoment, AggregationPeriod.Hour));
				}

				if (currentAggregationMoment.Hour == 0)
				{
					collectValues.AddRange(await GetAggregated(_dayRules, currentAggregationMoment, AggregationPeriod.Day));
				}

				CollectValues?.Invoke(this, collectValues);
			}
			catch (Exception ex)
			{
				_logger.LogWarning("Ошибка при расчете агрегатных значений: {message}", ex.Message);
			}
			finally
			{
				var nextMinute = currentAggregationMoment.RoundToFrequency(TagFrequency.ByMinute).AddMinutes(1);
				var now = DateFormats.GetCurrentDateTime();
				var delay = nextMinute - now;
				_logger.LogInformation("Ожидание следующего выполнения: {delay}", delay);
				await Task.Delay(delay);
			}
		}
	}

	private async Task<List<CollectValue>> GetAggregated(TagAggregationRule[] rules, DateTime date, AggregationPeriod period)
	{
		var aggregated = await ValuesRepository.GetWeightedAggregated(_db, rules.Select(x => x.TagSourceId).ToArray(), date, period);

		_tagsStateService.UpdateTagState([
			new() {
				RequestKey = "aggregate-collector",
				Tags = rules.Select(x => x.TagSourceGuid).ToArray()
			}
		]);

		return aggregated
			.Select(value => new
			{
				Value = value,
				Tag = rules.FirstOrDefault(x => x.TagSourceId == value.TagId),
			})
			.Where(x => x.Tag != null)
			.Select(x => new CollectValue
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
	}

	class TagAggregationRule
	{
		public required int Id { get; set; }

		public required Guid Guid { get; set; }

		public required string Name { get; set; }

		public required int TagSourceId { get; set; }

		public required Guid TagSourceGuid { get; set; }

		public required TagAggregation Type { get; set; }

		public required AggregationPeriod Period { get; set; }
	}

	#endregion
}
