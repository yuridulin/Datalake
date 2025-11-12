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
public class InopcCollector(
	IReceiverService receiverService,
	IDataCollectorWriter writer,
	ILogger<DatalakeCollector> logger,
	SourceSettingsDto source) : DataCollectorBase(writer, logger, source, 5000)
{
	public override Task StartAsync(CancellationToken cancellationToken)
	{
		if (source.RemoteSettings == null)
			return NotStartAsync("нет настроек получения данных");

		if (string.IsNullOrEmpty(source.RemoteSettings.RemoteHost))
			return NotStartAsync("адрес для получения данных пуст");

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

		if (itemsToSend.Count == 0)
			return NotStartAsync("нет тегов для получения данных");

		return base.StartAsync(cancellationToken);
	}

	#region Реализация
	private List<Item> itemsToSend = [];
	private Dictionary<string, List<TagSettingsDto>> itemsTags = [];

	protected override async Task ExecuteAsync(CollectorUpdate state, CancellationToken cancellationToken)
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
			if (response.IsConnected)
			{
				var itemsValues = response.Tags.ToDictionary(x => x.Name, x => x);
				now = DateTimeExtension.GetCurrentDateTime();

				state.Values = response.Tags
					.SelectMany(item => itemsTags[item.Name]
						.Select(tag => TagValue.FromRaw(tag.TagId, tag.TagType, now, item.Quality, item.Value, tag.ScaleSettings?.GetScale())))
					.ToList();
				state.IsActive = true;
			}

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
