using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Data.Application.Interfaces;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Application.Models.Tags;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.DataCollectors;

[Transient]
public class InopcCollector : DataCollectorBase
{
	public InopcCollector(
		IReceiverService receiverService,
		IDataCollectorProcessor processor,
		ILogger<DatalakeCollector> logger,
		SourceSettingsDto source) : base(processor, logger, source)
	{
		this.receiverService = receiverService;

		itemsToSend = [];
		itemsTags = [];

		Dictionary<string, TagResolution> uniqueItems = [];
		foreach (var tag in source.NotDeletedTags)
		{
			if (tag.InopcSettings == null)
				continue;

			string? item = tag.InopcSettings.RemoteItem;
			if (string.IsNullOrWhiteSpace(item))
				continue;

			if (!uniqueItems.TryGetValue(item, out var resolution))
			{
				uniqueItems[item] = tag.TagResolution;
			}
			else if (resolution > tag.TagResolution)
			{
				uniqueItems[item] = tag.TagResolution;
			}

			if (!itemsTags.TryGetValue(item, out var tagsList))
			{
				tagsList = [];
				itemsTags[item] = tagsList;
			}
			tagsList.Add(tag);
		}

		itemsToSend = uniqueItems
			.Select(kv => new Item { TagName = kv.Key, Resolution = kv.Value, LastAsk = DateTime.MinValue })
			.ToList();
	}

	public override Task StartAsync(CancellationToken stoppingToken)
	{
		if (source.RemoteSettings == null)
		{
			logger.LogWarning("Сборщик {name} не имеет настроек получения данных и не будет запущен", Name);
			return Task.CompletedTask;
		}

		if (string.IsNullOrEmpty(source.RemoteSettings.RemoteHost))
		{
			logger.LogWarning("Сборщик {name} не имеет адреса для получения данных и не будет запущен", Name);
			return Task.CompletedTask;
		}

		if (itemsToSend.Count == 0)
		{
			logger.LogWarning("Сборщик {name} не имеет значений для запроса и не будет запущен", Name);
			return Task.CompletedTask;
		}

		return base.StartAsync(stoppingToken);
	}


	#region Реализация

	private readonly List<Item> itemsToSend;
	private readonly IReceiverService receiverService;
	private readonly Dictionary<string, List<TagSettingsDto>> itemsTags;

	protected override async Task WorkAsync(CancellationToken cancellationToken)
	{
		var now = DateTimeExtension.GetCurrentDateTime();
		List<Item> tags = [];

		foreach (var item in itemsToSend)
		{
			if (item.LastAsk == null)
			{
				tags.Add(item);
			}
			else
			{
				var needToAsk = item.LastAsk.Value.AddByResolution(item.Resolution) <= now;

				if (needToAsk)
					tags.Add(item);
			}
		}

		if (tags.Count > 0)
		{
			var items = tags.Select(x => x.TagName).ToArray();

			var response = await receiverService.AskInopc(items, source.RemoteSettings!.RemoteHost);
			var itemsValues = response.Tags.ToDictionary(x => x.Name, x => x);
			now = DateTimeExtension.GetCurrentDateTime();

			var collectedValues = response.Tags
				.SelectMany(item => itemsTags[item.Name]
					.Select(tag => TagValue.FromRaw(tag.TagId, tag.TagType, now, item.Quality, item.Value, tag.ScaleSettings?.GetScale())))
				.ToArray();

			await WriteValuesAsync(collectedValues, cancellationToken);

			foreach (var tag in tags.Where(x => response.Tags.Select(t => t.Name).Contains(x.TagName)))
			{
				tag.LastAsk = now;
			}
		}
	}

	private record class Item
	{
		public required string TagName { get; set; }

		public DateTime? LastAsk { get; set; }

		public TagResolution Resolution { get; set; }
	}

	#endregion
}
