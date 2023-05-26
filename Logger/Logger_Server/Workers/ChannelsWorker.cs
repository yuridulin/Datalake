using LinqToDB;
using LinqToDB.Data;
using Logger.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Logger.Workers
{
	public static class ChannelsWorker
	{
		static List<Channel> Channels { get; set; } = new List<Channel>();

		static DateTime LastUpdate { get; set; } = DateTime.MinValue;

		static List<int> CheckedLogs { get; set; } = new List<int>();

		static List<Rel_Log_WebView> LogsToWebView { get; set; } = new List<Rel_Log_WebView>();

		static List<Rel_Log_Telegram> LogsToTelegram { get; set; } = new List<Rel_Log_Telegram>();

		public static async Task Work(CancellationToken token)
		{
			using (var db = new DatabaseContext())
			{
				while (!token.IsCancellationRequested)
				{
					// проверка, не изменилась ли конфигурация
					var currentUpdate = await db.Settings
						.Select(x => x.LastUpdate)
						.FirstAsync();

					if (currentUpdate > LastUpdate)
					{
						await ReloadChannels(db);
						LastUpdate = currentUpdate;
					}

					// получение пачки логов для обработки
					var uncheckedLogs = await db.Logs
						.Where(x => !x.Checked)
						.OrderBy(x => x.TimeStamp)
						.Take(100)
						.ToListAsync();

					if (uncheckedLogs.Count > 0)
					{
						Console.WriteLine("ChannelsWorker: get logs to process: " + uncheckedLogs.Count);
					}

					// обработка каждого лога:
					//   ищем каналы, связанные с фильтром, перехватившим это сообщение
					//   для каждого канала в зависимости от его типа производим обработку
					//   если всё хорошо, записываем лог в лист как обработанный
					foreach (var log in uncheckedLogs)
					{
						var requiredChannels = Channels.Where(x => x.Filters.Any(y => y.Id == log.LogFilterId));

						foreach (var channel in requiredChannels) 
						{
							// web просмотр
							// отдельная таблица, по которой устанавливается, какие логи нужно отображать в веб-вью
							// на будущее - указание конкретных веб-вью, чтобы делать отчеты
							// (либо отдельные отчеты, каждый из которых будет фильтром над общим списком логов)
							if (channel.Type == "web")
							{
								LogsToWebView.Add(new Rel_Log_WebView
								{
									LogId = log.Id,
									ChannelId = channel.Id
								});
							}

							// отправка сообщения в канал Telegram
							if (channel.Type == "telegram")
							{
								LogsToTelegram.Add(new Rel_Log_Telegram 
								{
									LogId = log.Id,
									ChannelId = log.Id,
									IsSended = false,
								});
							}
						}

						CheckedLogs.Add(log.Id);
					}

					// все обработанные логи обновляем в базе
					await db.Logs
						.Where(x => CheckedLogs.Contains(x.Id))
						.Set(x => x.Checked, true)
						.UpdateAsync();
					CheckedLogs.Clear();

					// обработка всех логов, которые должны быть в веб-вью
					await db.BulkCopyAsync(LogsToWebView);
					LogsToWebView.Clear();

					// обработка всех сообщений, которые нужно отослать
					await db.BulkCopyAsync(LogsToTelegram);
					LogsToTelegram.Clear();

					// ожидание следующего подхода
					Task.Delay(TimeSpan.FromSeconds(5)).Wait();
				}
			}
		}

		static async Task ReloadChannels(DatabaseContext db)
		{
			var query = from c in db.Channels
						from r in db.Rel_LogFilter_Channel.LeftJoin(x => x.ChannelId == c.Id)
						from f in db.LogsFilters.LeftJoin(x => x.Id == r.LogFilterId)
						select new
						{
							c,
							f
						};

			var channels = await query
				.ToListAsync();
			
			Channels = channels
				.GroupBy(x => x.c)
				.Select(g => new Channel
				{
					Id = g.Key.Id,
					Name = g.Key.Name,
					Type = g.Key.Type,
					Filters = g
						.Where(x => x.f != null)
						.Select(x => x.f)
						.ToList()
				})
				.ToList();
		}
	}
}
