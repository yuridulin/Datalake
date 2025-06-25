using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.Services.Receiver;
using Datalake.Server.Services.StateManager;

namespace Datalake.Server.BackgroundServices.Collector.Collectors;

internal class OldDatalakeCollector : CollectorBase
{
	public OldDatalakeCollector(
		ReceiverService receiverService,
		SourcesStateService sourcesStateService,
		SourceWithTagsInfo source,
		ILogger<OldDatalakeCollector> logger) : base(source.Name, source, logger)
	{
		_id = source.Id;
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
			.ToDictionary(x => x.Guid, x => new ValueWriteRequest
			{
				Value = null,
				Quality = TagQuality.Unknown,
				Date = DateTime.MinValue,
				Guid = x.Guid,
			});
	}

	public override void Start(CancellationToken stoppingToken)
	{
		if (_itemsToSend.Count == 0)
		{
			_logger.LogWarning("Сборщик \"{name}\" не имеет значений для запроса и не будет запущен", _name);
			return;
		}

		Task.Run(Work, stoppingToken);

		base.Start(stoppingToken);
	}


	#region Реализация

	private readonly int _id;
	private readonly string _address;
	private readonly List<Item> _itemsToSend;
	private readonly ReceiverService _receiverService;
	private readonly SourcesStateService _stateService;
	private readonly CancellationTokenSource _tokenSource;
	private readonly Dictionary<Guid, ValueWriteRequest> _previousValues;

	private async Task Work()
	{
		List<ValueWriteRequest> collectedValues;
		List<Item> updatedItems;

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
							collectedValues.AddRange(item.Tags.Select(guid => new ValueWriteRequest
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

					await WriteAsync(collectedValues);

					foreach (var tag in updatedItems)
					{
						tag.LastAsk = response.Timestamp;
					}
				}

				_stateService.UpdateSource(_id, connected: true);
			}
			catch (Exception ex)
			{
				_logger.LogWarning("Ошибка в сборщике {name}: {message}", _name, ex.Message);
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
