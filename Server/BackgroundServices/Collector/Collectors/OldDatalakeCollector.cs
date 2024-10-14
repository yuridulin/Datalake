using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Models.Sources;
using Datalake.Database;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.BackgroundServices.Collector.Models;
using Datalake.Server.Services.Receiver;

namespace Datalake.Server.BackgroundServices.Collector.Collectors;

internal class OldDatalakeCollector : CollectorBase
{
	public OldDatalakeCollector(
		ReceiverService receiverService,
		SourceWithTagsInfo source,
		ILogger<OldDatalakeCollector> logger) : base(source, logger)
	{
		_id = source.Id;
		_logger = logger;
		_receiverService = receiverService;
		_address = source.Address ?? throw new InvalidOperationException();
		_tokenSource = new CancellationTokenSource();

		_itemsToSend = source.Tags
			.Where(x => !string.IsNullOrEmpty(x.Item))
			.GroupBy(x => x.Item)
			.Select(g => new Item
			{
				TagName = g.Key!,
				PeriodInSeconds = g.Select(x => x.Interval).Min(),
				LastAsk = DateTime.MinValue,
				Tags = g.Select(x => x.Guid).ToArray(),
			})
			.ToList();

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

	private readonly int _id;
	private readonly string _address;
	private readonly List<Item> _itemsToSend;
	private readonly ReceiverService _receiverService;
	private readonly CancellationTokenSource _tokenSource;
	private ILogger<OldDatalakeCollector> _logger;

	private async Task Work()
	{
		List<CollectValue> collectedValues;
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
					var response = await _receiverService.AskOldDatalake([.. items.Select(x => x.TagName)], _address);

					foreach (var value in response.Tags)
					{
						var item = items.FirstOrDefault(x => x.TagName == value.Name);
						if (item != null)
						{
							collectedValues.AddRange(item.Tags.Select(guid => new CollectValue
							{
								DateTime = response.Timestamp,
								Name = value.Name,
								Quality = value.Quality,
								Guid = guid,
								Value = value.Value,
							}));

							updatedItems.Add(item);
						}
					}

					CollectValues?.Invoke(this, collectedValues);

					foreach (var tag in updatedItems)
					{
						tag.LastAsk = response.Timestamp;
					}

					_logger.LogDebug("Опрос Datalake (old) [{id}][{address}], получено значений: {count}",
						_id, _address, response.Tags.Length);
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning("Ошибка в сборщике Datalake (old) [{id}]: {message}", _id, ex.Message);
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
