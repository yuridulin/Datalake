using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.Services.Receiver;
using Datalake.Server.Services.StateManager;
using System.Collections.Concurrent;

namespace Datalake.Server.BackgroundServices.Collector.Collectors;

internal class InopcCollector : CollectorBase
{
	public InopcCollector(
		ReceiverService receiverService,
		SourcesStateService sourcesStateService,
		SourceWithTagsInfo source,
		ILogger<InopcCollector> logger) : base(source.Name, source, logger)
	{
		_receiverService = receiverService;
		_stateService = sourcesStateService;
		_address = source.Address ?? throw new InvalidOperationException();
		_tokenSource = new CancellationTokenSource();
		_id = source.Id;

		_itemsToSend = source.Tags
			.Where(x => !string.IsNullOrEmpty(x.Item))
			.DistinctBy(x => x.Item)
			.Select(x => new Item
			{
				TagName = x.Item!,
				Frequency = x.Frequency,
				LastAsk = DateTime.MinValue
			})
			.ToList();

		_itemsTags = source.Tags
			.Where(x => x.Item != null)
			.GroupBy(x => x.Item)
			.ToDictionary(g => g.Key!, g => g.Select(x => x.Guid).ToArray());

		_previousValues = new ConcurrentDictionary<Guid, ValueWriteRequest>(source.Tags
			.ToDictionary(x => x.Guid, x => new ValueWriteRequest
			{
				Value = null,
				Quality = TagQuality.Unknown,
				Date = DateTime.MinValue,
				Guid = x.Guid,
			}));
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
	private readonly Dictionary<string, Guid[]> _itemsTags;
	private readonly CancellationTokenSource _tokenSource;
	private readonly ConcurrentDictionary<Guid, ValueWriteRequest> _previousValues;

	private async Task Work()
	{
		_logger.LogDebug("Старт опроса INOPC [{id}][{address}]", _id, _address);

		while (!_tokenSource.Token.IsCancellationRequested)
		{
			try
			{
				var now = DateFormats.GetCurrentDateTime();
				List<Item> tags = [];

				foreach (var item in _itemsToSend)
				{
					if (item.LastAsk == null)
					{
						tags.Add(item);
					}
					else
					{
						var diff = (now - item.LastAsk.Value).TotalSeconds;

						var needToAsk = item.Frequency switch
						{
							TagFrequency.NotSet => true,
							TagFrequency.ByMinute => diff >= 60,
							TagFrequency.ByHour => diff >= 3600,
							TagFrequency.ByDay => diff >= 86400,
						};

						if (needToAsk)
							tags.Add(item);
					}
				}

				if (tags.Count > 0)
				{
					var items = tags.Select(x => x.TagName).ToArray();

					var response = await _receiverService.AskInopc(items, _address, true);
					var itemsValues = response.Tags.ToDictionary(x => x.Name, x => x);
					now = DateFormats.GetCurrentDateTime();

					var collectedValues = response.Tags
						.SelectMany(item => _itemsTags[item.Name]
							.Select(guid => new ValueWriteRequest
							{
								Date = now,
								Name = item.Name,
								Quality = item.Quality,
								Guid = guid,
								Value = item.Value,
							}))
						.ToArray();

					collectedValues = collectedValues
						.Where(x =>
							x.Value != _previousValues[x.Guid!.Value].Value &&
							x.Quality != _previousValues[x.Guid!.Value].Quality)
						.ToArray();

					foreach (var v in collectedValues)
						_previousValues[v.Guid!.Value] = v;

					await WriteAsync(collectedValues);

					foreach (var tag in tags.Where(x => response.Tags.Select(t => t.Name).Contains(x.TagName)))
					{
						tag.LastAsk = now;
					}
				}

				_stateService.UpdateSource(_id, connected: true);
			}
			catch (Exception ex)
			{
				_logger.LogWarning("Ошибка в сборщике INOPC [{id}]: {message}", _id, ex.Message);
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

		public DateTime? LastAsk { get; set; }

		public TagFrequency Frequency { get; set; }
	}

	#endregion
}
