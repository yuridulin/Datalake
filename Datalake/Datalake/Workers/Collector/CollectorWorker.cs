using Datalake.Database;
using Datalake.Database.Enums;
using Datalake.Web;
using Newtonsoft.Json.Linq;
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
			using (var db = new DatabaseContext())
			{
				try
				{
					db.Log(Name, "Выполняется пересборка пакетов обновления", ProgramLogType.Warning);

					var lastUpdate = db.GetUpdateDate();
					if (lastUpdate == StoredUpdate) return;

					var sources = db.Sources.ToList();
					var tags = db.Tags.Where(x => sources.Select(s => s.Id).Contains(x.SourceId)).ToList();

					db.Log(Name, "Количество тегов: " + tags.Count, ProgramLogType.Trace);

					foreach (var tag in tags) tag.PrepareToCollect();

					Packets = sources
						.ToDictionary(x => x.Address, x => tags.Where(t => t.SourceId == x.Id).ToList());

					db.Log(Name, "Количество пакетов обновления: " + Packets.Count, ProgramLogType.Trace);

					StoredUpdate = lastUpdate;
				}
				catch (Exception ex)
				{
					db.Log(Name, ex.Message, ProgramLogType.Error);
				}
				finally
				{
					db.Log(Name, "Пересборка пакетов обновления завершена", ProgramLogType.Warning);
				}
			}
		}

		static void Update()
		{
			using (var db = new DatabaseContext())
			{
				try
				{
					db.Log(Name, "Запущено обновление тегов", ProgramLogType.Trace);

					foreach (var packet in Packets)
					{
						var now = DateTime.Now;
						var tagsToUpdate = packet.Value
							.Where(x => x.IsNeedToUpdate(now))
							.ToList();

						db.Log(Name, "Количество тегов, ожидающих обновления: " + tagsToUpdate.Count, ProgramLogType.Trace);

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
							db.Log(Name, "Запрос к серверу: " + packet.Key, ProgramLogType.Trace);

							var res = Inopc.AskInopc(items, packet.Key);

							db.Log(Name, "Количество полученных от сервера значений: " + res.Tags.Length, ProgramLogType.Trace);

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

									db.Log(Name, "Записано новое значение тега [" + tag.Name + "]: " + raw, ProgramLogType.Trace);
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

									db.Log(Name, "Новое значение тега [" + tag.Name + "] не найдено", ProgramLogType.Warning);
								}

							}

						}
						catch (Exception ex)
						{
							db.Log(Name, ex.Message, ProgramLogType.Error);
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
				catch (Exception ex)
				{
					db.Log(Name, ex.Message, ProgramLogType.Error);
				}
				finally
				{
					db.Log(Name, "Обновление тегов завершено", ProgramLogType.Trace);
				}
			}
		}
	}
}
