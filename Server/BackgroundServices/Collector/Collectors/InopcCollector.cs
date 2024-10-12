using Datalake.ApiClasses.Models.Sources;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.BackgroundServices.Collector.Models;
using Datalake.Server.Services.Receiver;

namespace Datalake.Server.BackgroundServices.Collector.Collectors;

internal class InopcCollector : CollectorBase
{
	public InopcCollector(
		ReceiverService receiverService,
		SourceWithTagsInfo source,
		ILogger<InopcCollector> logger) : base(source, logger)
	{
		_logger = logger;
		_receiverService = receiverService;
		_address = source.Address ?? throw new InvalidOperationException();
		_tokenSource = new CancellationTokenSource();
		_id = source.Id;

		_itemsToSend = source.Tags
			.Where(x => !string.IsNullOrEmpty(x.Item))
			.DistinctBy(x => x.Item)
			.Select(x => new Item
			{
				TagName = x.Item!,
				PeriodInSeconds = x.Interval,
				LastAsk = DateTime.MinValue
			})
			.ToList();

		_itemsTags = source.Tags
			.Where(x => x.Item != null)
			.GroupBy(x => x.Item)
			.ToDictionary(g => g.Key!, g => g.Select(x => x.Guid).ToArray());

		_logger.LogDebug("Create iNOPC collector {address}. Tags: {count}", _address, _itemsToSend.Count);
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
	private readonly Dictionary<string, Guid[]> _itemsTags;
	private readonly CancellationTokenSource _tokenSource;
	private ILogger<InopcCollector> _logger;

	private async Task Work()
	{
		while (!_tokenSource.Token.IsCancellationRequested)
		{
			try
			{
				var now = DateTime.Now;
				List<Item> tags = [];

				foreach (var item in _itemsToSend)
				{
					if (item.PeriodInSeconds == 0)
					{
						tags.Add(item);
					}
					else
					{
						var diff = (now - item.LastAsk).TotalSeconds;
						if (diff > item.PeriodInSeconds)
						{
							tags.Add(item);
						}
					}
				}

				if (tags.Count > 0)
				{
					var items = tags.Select(x => x.TagName).ToArray();

					var response = await _receiverService.AskInopc(items, _address);
					var itemsValues = response.Tags.ToDictionary(x => x.Name, x => x);

					var collectedValues = response.Tags
						.SelectMany(item => _itemsTags[item.Name]
							.Select(guid => new CollectValue
							{
								DateTime = response.Timestamp,
								Name = item.Name,
								Quality = item.Quality,
								Guid = guid,
								Value = item.Value,
							}))
						.ToArray();

					CollectValues?.Invoke(this, collectedValues);

					foreach (var tag in tags.Where(x => response.Tags.Select(t => t.Name).Contains(x.TagName)))
					{
						tag.LastAsk = response.Timestamp;
					}

					_logger.LogDebug("Опрос INOPC [{id}][{address}], получено значений: {count}", _id, _address, response.Tags.Length);
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning("Ошибка в сборщике INOPC [{id}]: {message}", _id, ex.Message);
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
	}

	#endregion
}
