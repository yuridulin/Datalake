using DatalakeApp.BackgroundSerivces.Collector.Collectors.Abstractions;
using DatalakeApp.Services.Receiver;
using DatalakeDatabase.Models;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace DatalakeApp.BackgroundSerivces.Collector.Collectors;

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
			.Select(x => new Item
			{
				TagName = x.SourceItem!,
				PeriodInSeconds = x.Interval,
				LastAsk = DateTime.MinValue
			})
			.ToList();

		_itemsTagId = source.Tags
			.Where(x => x.SourceItem != null)
			.ToDictionary(x => x.SourceItem!, x => x.Id);

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
	private readonly Dictionary<string, int> _itemsTagId;
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

		CollectValues?.Invoke(this, response.Tags
			.Select(x => new Models.CollectValue
			{
				DateTime = response.Timestamp,
				Name = x.Name,
				Quality = x.Quality,
				TagId = _itemsTagId[x.Name],
				Value = x.Value,
			})
		);

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
