using DatalakeDatabase.Models;
using DatalakeServer.BackgroundServices.Collector.Collectors.Abstractions;
using DatalakeServer.Services.Receiver;
using Timer = System.Timers.Timer;

namespace DatalakeServer.BackgroundServices.Collector.Collectors;

public class InopcCollector : CollectorBase
{
	public InopcCollector(
		ReceiverService receiverService,
		Source source,
		ILogger<InopcCollector> logger) : base(source, logger)
	{
		_logger = logger;
		_receiverService = receiverService;
		_timer = new Timer();
		_address = source.Address ?? throw new InvalidOperationException();

		_itemsToSend = source.Tags
			.Where(x => !string.IsNullOrEmpty(x.SourceItem))
			.DistinctBy(x => x.SourceItem)
			.Select(x => new Item
			{
				TagName = x.SourceItem!,
				PeriodInSeconds = x.Interval,
				LastAsk = DateTime.MinValue
			})
			.ToList();

		_itemsTags = source.Tags
			.Where(x => x.SourceItem != null)
			.GroupBy(x => x.SourceItem)
			.ToDictionary(g => g.Key!, g => g.Select(x => x.Id).ToArray());

		_timer.Elapsed += async (s, e) => await Timer_ElapsedAsync();
		_timer.Interval = _itemsToSend
			.Where(x => x.PeriodInSeconds > 0)
			.Select(x => x.PeriodInSeconds)
			.DefaultIfEmpty(1)
			.Min() * 1000;

		_logger.LogInformation("Collector for source {name} has {count} tags", source.Name, _itemsToSend.Count);
	}

	public override event CollectEvent? CollectValues;

	public override Task Start()
	{
		if (_itemsToSend.Count > 0)
		{
			_timer.Start();
			Task.Run(Timer_ElapsedAsync);
		}

		return base.Start();
	}

	public override Task Stop()
	{
		if (_itemsToSend.Count > 0)
		{
			_timer.Stop();
		}

		return base.Stop();
	}


	#region Реализация

	private readonly ReceiverService _receiverService;
	private readonly Timer _timer;
	private readonly string _address;
	private readonly List<Item> _itemsToSend;
	private readonly Dictionary<string, int[]> _itemsTags;
	private ILogger<InopcCollector> _logger;

	private async Task Timer_ElapsedAsync()
	{
		_logger.LogInformation("Collect from {address}", _address);

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
		if (tags.Count == 0)
			return;

		var items = tags.Select(x => x.TagName).ToArray();

		var response = await _receiverService.AskInopc(items, _address);
		var itemsValues = response.Tags.ToDictionary(x => x.Name, x => x);

		CollectValues?.Invoke(this, response.Tags
			.SelectMany(item => _itemsTags[item.Name]
				.Select(id => new Models.CollectValue
				{
					DateTime = response.Timestamp,
					Name = item.Name,
					Quality = item.Quality,
					TagId = id,
					Value = item.Value,
				})
			));

		foreach (var tag in tags.Where(x => response.Tags.Select(t => t.Name).Contains(x.TagName)))
		{
			tag.LastAsk = response.Timestamp;
		}

		_logger.LogInformation("Collect from {address} completed, update {count} values", _address, response.Tags.Length);
	}

	class Item
	{
		public required string TagName { get; set; }

		public DateTime LastAsk { get; set; }

		public int PeriodInSeconds { get; set; }
	}

	#endregion
}
