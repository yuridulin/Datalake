using Datalake.Database;
using Datalake.Database.Enums;
using Datalake.Web;
using Datalake.Workers.Logs;
using Datalake.Workers.Logs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers.Collector
{
	public static class CollectorWorker
	{
		public static async Task Start(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				Rebuild();
				Update();

				await Task.Delay(1000);
			}
		}

		static string Name = nameof(Collector);

		static DateTime StoredUpdate { get; set; } = DateTime.MinValue;

		static Dictionary<string, List<Tag>> Packets { get; set; } = new Dictionary<string, List<Tag>>();

		static void Rebuild()
		{
			if (Cache.LastUpdate <= StoredUpdate) return;

			LogsWorker.Add(Name, "Выполняется пересборка пакетов обновления", LogType.Warning);

			using (var db = new DatabaseContext())
			{
				var sources = db.Sources.ToList();
				var tags = db.Tags.Where(x => sources.Select(s => s.Id).Contains(x.SourceId)).ToList();

				foreach (var tag in tags) tag.PrepareToCollect();

				Packets = sources
					.ToDictionary(x => x.Address, x => tags.Where(t => t.SourceId == x.Id).ToList());

				StoredUpdate = Cache.LastUpdate;

				LogsWorker.Add(Name, "Найдено тегов: " + tags.Count, LogType.Trace);
				LogsWorker.Add(Name, "Новое количество пакетов обновления: " + Packets.Count, LogType.Trace);
			}
		}

		static void Update()
		{
			LogsWorker.Add(Name, "Обновление", LogType.Trace);

			var values = new List<TagHistory>();

			foreach (var packet in Packets)
			{
				var now = DateTime.Now;
				var tagsToUpdate = packet.Value
					.Where(x => x.IsNeedToUpdate(now))
					.ToList();

				if (tagsToUpdate.Count == 0) continue;

				var ids = tagsToUpdate
					.Select(x => x.Id)
					.ToList();

				var items = tagsToUpdate
					.Select(x => x.SourceItem)
					.Distinct()
					.ToArray();

				var res = Inopc.AskInopc(items, packet.Key);

				LogsWorker.Add(Name, "Запрос к серверу: " + packet.Key + ", ожидается " + tagsToUpdate.Count + ", получено " + res.Tags.Length, LogType.Trace);

				foreach (var tag in tagsToUpdate)
				{
					TagHistory value;
					var inopcTag = res.Tags.FirstOrDefault(x => x.Name == tag.SourceItem);

					if (inopcTag != null)
					{
						var (text, number, quality) = tag.FromRaw(inopcTag.Value, inopcTag.Quality);

						value = new TagHistory
						{
							TagId = tag.Id,
							Date = res.Timestamp,
							Text = text,
							Number = number,
							Quality = quality,
							Type = tag.Type,
							Using = TagHistoryUse.Basic,
						};
					}
					else
					{
						value = new TagHistory
						{
							TagId = tag.Id,
							Date = res.Timestamp,
							Text = null,
							Number = null,
							Quality = TagQuality.Bad_NoConnect,
							Type = tag.Type,
							Using = TagHistoryUse.Basic,
						};
					}

					if (Cache.IsNew(value)) values.Add(value);
				}

				foreach (var tag in packet.Value)
				{
					if (ids.Contains(tag.Id)) tag.SetAsUpdated(now);
				}
			}

			if (values.Count > 0)
			{
				using (var db = new DatabaseContext())
				{
					db.WriteToHistory(values);
				}
			}

			LogsWorker.Add(Name, "Записано значений: " + values.Count, LogType.Trace);
		}
	}
}
