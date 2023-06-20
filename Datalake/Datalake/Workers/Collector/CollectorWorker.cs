using Datalake.Database;
using Datalake.Database.Enums;
using Datalake.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers.Collector
{
	public class CollectorWorker
	{
		public static async Task Start(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					Rebuild();
					Update();
				}
				catch (Exception ex)
				{
					Console.WriteLine(DateTime.Now + " [" + nameof(CollectorWorker) + "] " + ex.ToString());
				}

				await Task.Delay(1000);
			}
		}

		static DateTime StoredUpdate { get; set; } = DateTime.MinValue;

		static Dictionary<string, List<Tag>> Packets { get; set; } = new Dictionary<string, List<Tag>>();

		static void Rebuild()
		{
			using (var db = new DatabaseContext())
			{
				var lastUpdate = db.GetUpdateDate();
				if (lastUpdate == StoredUpdate) return;

				Console.WriteLine("Выполняется пересборка пакетов обновления");

				var sources = db.Sources.ToList();
				var tags = db.Tags.Where(x => sources.Select(s => s.Id).Contains(x.SourceId)).ToList();

				foreach (var tag in tags) tag.PrepareToCollect();

				Packets = sources
					.ToDictionary(x => x.Address, x => tags.Where(t => t.SourceId == x.Id).ToList());

				StoredUpdate = lastUpdate;
			}
		}

		static void Update()
		{
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

				try
				{
					var res = Inopc.AskInopc(items, packet.Key);

					using (var db = new DatabaseContext())
					{
						foreach (var tag in tagsToUpdate)
						{
							var inopcTag = res.Tags.FirstOrDefault(x => x.Name == tag.SourceItem);

							if (inopcTag != null)
							{
								var (text, raw, number, quality) = tag.FromRaw(inopcTag.Value, inopcTag.Quality);

								db.WriteToHistory(new TagHistory
								{
									TagId = tag.Id,
									Date = res.Timestamp,
									Text = text,
									Raw = raw,
									Number = number,
									Quality = quality,
								});
							}
							else
							{
								db.WriteToHistory(new TagHistory
								{
									TagId = tag.Id,
									Date = res.Timestamp,
									Text = null,
									Number = null,
									Raw = null,
									Quality = TagQuality.Bad_NoConnect
								});
							}
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(DateTime.Now + " [" + nameof(CalculatorWorker) + "] " + ex.ToString());
				}
				finally
				{
					foreach (var tag in packet.Value)
					{
						if (ids.Contains(tag.Id)) tag.SetAsUpdated(now);
					}
				}
			}
		}
	}
}
