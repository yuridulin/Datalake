using DatalakeApp.BackgroundSerivces.Collector.Collectors.Abstractions;
using DatalakeApp.Services.Receiver;
using DatalakeDatabase.Models;
using Timer = System.Timers.Timer;

namespace DatalakeApp.BackgroundSerivces.Collector.Collectors;

public class InopcCollector : CollectorBase
{
	public InopcCollector(
		ReceiverService receiverService,
		Source source,
		ILogger<InopcCollector> logger) : base(source, logger)
	{
		_receiverService = receiverService;
		_timer = new Timer();
		_address = source.Address ?? throw new InvalidOperationException();

		_itemsToSend = source.Tags
			.Where(x => !string.IsNullOrEmpty(x.SourceItem))
			.Select(x => new Item
			{
				TagName = x.SourceItem!,
				Period = TimeSpan.FromSeconds(x.Interval),
				LastAsk = DateTime.MinValue
			})
			.ToList();

		_timer.Elapsed += async (s, e) => await Timer_ElapsedAsync();
	}

	public override event CollectEvent? CollectValues;

	public override Task Start()
	{
		_timer.Start();
		Task.Run(Timer_ElapsedAsync);

		return base.Start();
	}

	public override Task Stop()
	{
		_timer.Stop();

		return base.Stop();
	}


	#region Реализация

	private readonly ReceiverService _receiverService;
	private readonly Timer _timer;
	private readonly string _address;
	private readonly List<Item> _itemsToSend = [];
	private readonly Dictionary<string, int> _itemsTagId = [];

	private async Task Timer_ElapsedAsync()
	{
		var now = DateTime.UtcNow;
		var tags = _itemsToSend.Where(x => now - x.LastAsk > x.Period).ToList();

		var response = await _receiverService.AskInopc(tags.Select(x => x.TagName).ToArray(), _address);

		CollectValues?.Invoke(this, response.Tags
			.Select(x => new Models.CollectValue
			{
				DateTime = response.Timestamp,
				Name = x.Name,
				Quality = x.Quality,
				TagId = _itemsTagId[x.Name],
			})
		);
	}

	class Item
	{
		public required string TagName { get; set; }

		public DateTime LastAsk { get; set; }

		public TimeSpan Period { get; set; }
	}

	#endregion
}
