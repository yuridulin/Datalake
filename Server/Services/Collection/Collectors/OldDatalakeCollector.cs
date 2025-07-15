using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.Services.Collection.Abstractions;
using Datalake.Server.Services.Maintenance;
using Datalake.Server.Services.Receiver;
using Datalake.Database.Extensions;

namespace Datalake.Server.Services.Collection.Collectors;

internal class OldDatalakeCollector : CollectorBase
{
	public OldDatalakeCollector(
		ReceiverService receiverService,
		SourceWithTagsInfo source,
		SourcesStateService sourcesStateService,
		ILogger<OldDatalakeCollector> logger) : base(source.Name, source, sourcesStateService, logger)
	{
		_receiverService = receiverService;

		_itemsToSend = source.Tags
			.Where(x => !string.IsNullOrEmpty(x.Item))
			.GroupBy(x => x.Item)
			.Select(g => new Item
			{
				TagName = g.Key!,
				Resolution = g.MinBy(x => x.Resolution.GetSortOrder())?.Resolution ?? TagResolution.NotSet,
				LastAsk = DateTime.MinValue,
				Tags = g.Select(x => x.Guid).ToArray(),
			})
			.ToDictionary(x => x.TagName);
	}

	public override void Start(CancellationToken stoppingToken)
	{
		if (_itemsToSend.Count == 0)
		{
			Task.Run(() => WriteAsync([], false), stoppingToken);
			_logger.LogWarning("Сборщик \"{name}\" не имеет значений для запроса и не будет запущен", _name);
			return;
		}

		if (string.IsNullOrEmpty(_source.Address))
		{
			Task.Run(() => WriteAsync([], false), stoppingToken);
			_logger.LogWarning("Сборщик \"{name}\" не имеет адреса для получения данных и не будет запущен", _name);
			return;
		}

		Task.Run(() => WriteAsync([], true), stoppingToken);
		base.Start(stoppingToken);
	}


	#region Реализация

	private readonly Dictionary<string, Item> _itemsToSend;
	private readonly ReceiverService _receiverService;
	private List<ValueWriteRequest> collectedValues = [];

	protected override async Task Work()
	{
		var now = DateFormats.GetCurrentDateTime();
		var items = _itemsToSend
			.Where(x => x.Value.Resolution == TagResolution.NotSet || x.Value.Resolution.AddToDate(x.Value.LastAsk) <= now)
			.ToDictionary();

		if (items.Count > 0)
		{
			var response = await _receiverService.AskInopc([.. items.Select(x => x.Value.TagName)], _source.Address!);

			foreach (var value in response.Tags)
			{
				items.TryGetValue(value.Name, out var item);
				if (item != null)
				{
					collectedValues.AddRange(item.Tags.Select(guid => new ValueWriteRequest
					{
						Date = now,
						Name = value.Name,
						Quality = value.Quality,
						Guid = guid,
						Value = value.Value,
					}));

					item.LastAsk = now;
				}
			}

			await WriteAsync(collectedValues);
		}
	}

	class Item
	{
		public required string TagName { get; set; }

		public DateTime LastAsk { get; set; }

		public TagResolution Resolution { get; set; }

		public required Guid[] Tags { get; set; }
	}

	#endregion
}
