using Datalake.Database.Models;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.Services.Receiver;

namespace Datalake.Server.BackgroundServices.Collector.Collectors;

internal class OldDatalakeCollector : CollectorBase
{
	public OldDatalakeCollector(
		ReceiverService receiverService,
		Source source,
		ILogger<OldDatalakeCollector> logger) : base(source, logger)
	{
		_logger = logger;
		_receiverService = receiverService;
		_address = source.Address ?? throw new InvalidOperationException();
		_tokenSource = new CancellationTokenSource();

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
			.ToDictionary(g => g.Key!, g => g.Select(x => x.GlobalGuid).ToArray());

		_logger.LogDebug("Create old Datalake collector {address}. Tags: {count}", _address, _itemsToSend.Count);
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

	private readonly string _address;
	private readonly List<Item> _itemsToSend;
	private readonly ReceiverService _receiverService;
	private readonly CancellationTokenSource _tokenSource;
	private readonly Dictionary<string, Guid[]> _itemsTags;
	private ILogger<OldDatalakeCollector> _logger;

	private async Task Work()
	{
		while (!_tokenSource.Token.IsCancellationRequested)
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

			if (tags.Count == 0)
				return;

			_logger.LogDebug("Collect from {address}", _address);

			var items = tags.Select(x => x.TagName).ToArray();

			var response = await _receiverService.AskOldDatalake(items, _address);
			var itemsValues = response.Tags.ToDictionary(x => x.Name, x => x);

			CollectValues?.Invoke(this, response.Tags
				.SelectMany(item => _itemsTags[item.Name]
					.Select(guid => new Models.CollectValue
					{
						DateTime = response.Timestamp,
						Name = item.Name,
						Quality = item.Quality,
						Guid = guid,
						Value = item.Value,
					})
				));

			foreach (var tag in tags.Where(x => response.Tags.Select(t => t.Name).Contains(x.TagName)))
			{
				tag.LastAsk = response.Timestamp;
			}

			_logger.LogDebug("Collect from {address} completed, update {count} values", _address, response.Tags.Length);

			await Task.Delay(1000);
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
