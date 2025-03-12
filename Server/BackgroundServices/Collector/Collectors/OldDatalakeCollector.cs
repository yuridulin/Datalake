using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.BackgroundServices.Collector.Models;
using Datalake.Server.Services.Receiver;
using Datalake.Server.Services.StateManager;

namespace Datalake.Server.BackgroundServices.Collector.Collectors;

internal class OldDatalakeCollector : CollectorBase
{
	public OldDatalakeCollector(
		ReceiverService receiverService,
		SourcesStateService sourcesStateService,
		SourceWithTagsInfo source,
		ILogger<OldDatalakeCollector> logger) : base(source, logger)
	{
		_id = source.Id;
		_logger = logger;
		_receiverService = receiverService;
		_stateService = sourcesStateService;
		_address = source.Address ?? throw new InvalidOperationException();
		_tokenSource = new CancellationTokenSource();

		_itemsToSend = source.Tags
			.Where(x => !string.IsNullOrEmpty(x.Item))
			.GroupBy(x => x.Item)
			.Select(g => new Item
			{
				TagName = g.Key!,
				PeriodInSeconds = g
					.Select(x => x.Frequency switch
					{
						TagFrequency.NotSet => 0,
						TagFrequency.ByMinute => 60,
						TagFrequency.ByHour => 3600,
						TagFrequency.ByDay => 86400,
					})
					.Min(),
				LastAsk = DateTime.MinValue,
				Tags = g.Select(x => x.Guid).ToArray(),
			})
			.ToList();

		_previousValues = source.Tags
			.ToDictionary(x => x.Guid, x => new CollectValue
			{
				Value = null,
				Quality = TagQuality.Unknown,
				Date = DateTime.MinValue,
				Guid = x.Guid,
			});
	}

	public override event CollectEvent? CollectValues;

	public override Task Start(CancellationToken stoppingToken)
	{
		if (_itemsToSend.Count == 0)
			return Task.CompletedTask;

		Task.Run(Work, stoppingToken);

		return base.Start(stoppingToken);
	}

	public override Task Stop()
	{
		_tokenSource.Cancel();

		return base.Stop();
	}


	#region Реализация

	private readonly int _id;
	private readonly string _address;
	private readonly List<Item> _itemsToSend;
	private readonly ReceiverService _receiverService;
	private readonly SourcesStateService _stateService;
	private readonly CancellationTokenSource _tokenSource;
	private ILogger<OldDatalakeCollector> _logger;
	private readonly Dictionary<Guid, CollectValue> _previousValues;

	private async Task Work()
	{
		List<CollectValue> collectedValues;
		List<Item> updatedItems;

		_logger.LogDebug("Старт опроса Datalake (old) [{id}][{address}]", _id, _address);

		while (!_tokenSource.Token.IsCancellationRequested)
		{
			try
			{
				collectedValues = [];
				updatedItems = [];

				var now = DateFormats.GetCurrentDateTime();
				var items = _itemsToSend
					.Where(x => x.PeriodInSeconds == 0 || (now - x.LastAsk).TotalSeconds > x.PeriodInSeconds)
					.ToArray();

				if (items.Length > 0)
				{
					var response = await _receiverService.AskInopc([.. items.Select(x => x.TagName)], _address);

					foreach (var value in response.Tags)
					{
						var item = items.FirstOrDefault(x => x.TagName == value.Name);
						if (item != null)
						{
							collectedValues.AddRange(item.Tags.Select(guid => new CollectValue
							{
								Date = DateFormats.GetCurrentDateTime(),
								Name = value.Name,
								Quality = value.Quality,
								Guid = guid,
								Value = value.Value,
							}));

							updatedItems.Add(item);
						}
					}

					collectedValues = collectedValues.Where(x => x != _previousValues[x.Guid!.Value]).ToList();
					foreach (var v in collectedValues)
						_previousValues[v.Guid!.Value] = v;

					CollectValues?.Invoke(this, collectedValues);

					foreach (var tag in updatedItems)
					{
						tag.LastAsk = response.Timestamp;
					}
				}

				_stateService.UpdateSource(_id, connected: true);
			}
			catch (Exception ex)
			{
				_logger.LogWarning("Ошибка в сборщике Datalake (old) [{id}]: {message}", _id, ex.Message);
				_stateService.UpdateSource(_id, connected: false);
			}
			finally
			{
				await Task.Delay(1000);
			}
		}
	}

	class Item
	{
		public required string TagName { get; set; }

		public DateTime LastAsk { get; set; }

		public int PeriodInSeconds { get; set; }

		public required Guid[] Tags { get; set; }
	}

	#endregion
}
