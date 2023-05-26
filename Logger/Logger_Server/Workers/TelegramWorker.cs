using LinqToDB;
using Logger.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Logger.Workers
{
	public static class TelegramWorker
	{
		static readonly string Token = "5615415781:AAGdakmBjFQVKpzajxfGYPu_ID8-Iv2pxrw";

		static readonly string ChatId = "-1001877460791";

		static List<int> SendedLogs { get; set; } = new List<int>();

		public static async Task Work(CancellationToken token)
		{
			string url = $"https://api.telegram.org/bot{Token}/sendMessage?chat_id={ChatId}&text=";

			while (!token.IsCancellationRequested)
			{
				// получение списка на передачу
				using (var db = new DatabaseContext())
				{
					var query = from r in db.Rel_Log_Telegram
								from l in db.Logs.LeftJoin(x => x.Id == r.LogId)
								where !r.IsSended
								select new
								{
									l.Id,
									l.Endpoint,
									l.TimeStamp,
									l.Type,
									l.Journal,
									l.Source,
									l.EventId,
									l.Message,
								};

					var logs = await query
						.Take(10)
						.ToListAsync();

					if (logs.Count > 0)
					{
						Console.WriteLine("{0}: {1}", "Telegram Worker", "logs to send = " + logs.Count);

						using (var webClient = new WebClient())
						{
							ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

							foreach (var log in logs)
							{
								string text = (log.Message.Length > 300 ? log.Message.Substring(0, 300) : log.Message)
									.Replace("\n", " ")
									.Replace("\t", "");
								string message = HttpUtility.UrlEncode($"{log.TimeStamp:dd.MM.yyyy HH:mm:ss}\n{log.Endpoint}\n{log.Type}\n\n{text}");

								try
								{
									webClient.DownloadString(url + message);

									SendedLogs.Add(log.Id);
									Console.WriteLine("{0}: {1}", "Telegram Worker", "log sended = " + log.Id);
								}
								catch (WebException e)
								{
									Console.WriteLine("{0}: {1}", "Telegram Worker", "error = [" + e.Status + "] " + e.Message + "\t" + url + message);
								}
								finally
								{
									Task.Delay(TimeSpan.FromSeconds(2)).Wait();
								}
							}
						}
					}

					if (SendedLogs.Count > 0)
					{
						await db.Rel_Log_Telegram
							.Where(x => SendedLogs.Contains(x.LogId))
							.Set(x => x.IsSended, true)
							.UpdateAsync();
					}

					SendedLogs.Clear();
				}

				// Ожидание следующего выполнения
				Task.Delay(TimeSpan.FromSeconds(5)).Wait();
			}
		}
	}
}