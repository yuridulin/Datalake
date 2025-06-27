using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.Services.Collection.Abstractions;
using Datalake.Server.Services.Maintenance;
using Datalake.Server.Services.Receiver;

namespace Datalake.Server.Services.Collection.Collectors;

internal class InopcCollector : CollectorBase
{
	public InopcCollector(
		ReceiverService receiverService,
		SourceWithTagsInfo source,
		SourcesStateService sourcesStateService,
		ILogger<InopcCollector> logger) : base(source.Name, source, sourcesStateService, logger)
	{
		_receiverService = receiverService;

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
			.ToDictionary(g => g.Key!, g => g.ToArray());
	}


	public override void Start(CancellationToken stoppingToken)
	{
		if (_itemsToSend.Count == 0)
		{
			_logger.LogWarning("Сборщик \"{name}\" не имеет значений для запроса и не будет запущен", _name);
			return;
		}

		if (string.IsNullOrEmpty(_source.Address))
		{
			_logger.LogWarning("Сборщик \"{name}\" не имеет адреса для получения данных и не будет запущен", _name);
			return;
		}

		Task.Run(Work, stoppingToken);

		base.Start(stoppingToken);
	}


	#region Реализация

	private readonly List<Item> _itemsToSend;
	private readonly ReceiverService _receiverService;
	private readonly Dictionary<string, SourceTagInfo[]> _itemsTags;

	protected override async Task Work()
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

			var response = await _receiverService.AskInopc(items, _source.Address!);
			var itemsValues = response.Tags.ToDictionary(x => x.Name, x => x);
			now = DateFormats.GetCurrentDateTime();

			var collectedValues = response.Tags
				.SelectMany(item => _itemsTags[item.Name]
					.Select(tagInfo => new ValueWriteRequest
					{
						Id = tagInfo.Id,
						Name = tagInfo.Name,
						Guid = tagInfo.Guid,
						Date = now,
						Quality = item.Quality,
						Value = item.Value,
					}))
				.ToArray();

			await WriteAsync(collectedValues, response.IsConnected);

			foreach (var tag in tags.Where(x => response.Tags.Select(t => t.Name).Contains(x.TagName)))
			{
				tag.LastAsk = now;
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
