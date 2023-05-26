using Datalake.Collector.Models;
using Datalake.Database;
using Datalake.Database.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Collector
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

				var tags = db.Tags.ToList();
				var sources = db.Sources.ToList();

				foreach (var tag in tags) tag.Prepare();

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
					var res = AskInopc(items, packet.Key);

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
					Console.WriteLine(DateTime.Now + " [" + nameof(CollectorWorker) + "] " + ex.ToString());
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

		public static DatalakeResponse AskInopc(string[] tags, string address)
		{
			DatalakeRequest req = new DatalakeRequest
			{
				Tags = tags
			};

			DatalakeResponse res;

			try
			{
				var web = (HttpWebRequest)WebRequest.Create("http://" + address + ":81/api/storage/read");

				web.ContentType = "application/json";
				web.Method = "POST";
				web.Timeout = 1000;

				using (var stream = web.GetRequestStream())
				{
					using (var streamWriter = new StreamWriter(stream))
					{
						string json = JsonConvert.SerializeObject(req);
						streamWriter.Write(json);
					}
				}

				using (var response = (HttpWebResponse)web.GetResponse())
				{
					using (var stream = response.GetResponseStream())
					{
						using (var streamReader = new StreamReader(stream))
						{
							string json = streamReader.ReadToEnd();
							res = JsonConvert.DeserializeObject<DatalakeResponse>(json);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(DateTime.Now + " [" + nameof(CollectorWorker) + "] " + ex.ToString());
				res = new DatalakeResponse
				{
					Timestamp = DateTime.Now,
					Tags = new InopcTag[0]
				};
			}

			return res;
		}
	}
}
