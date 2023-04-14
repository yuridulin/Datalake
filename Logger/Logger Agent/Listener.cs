using Logger_Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Logger_Agent
{
	public static class Listener
	{
		static Config Config { get; set; }

		static List<Log> Logs { get; set; } = new List<Log>();

		public static async void Start(CancellationToken token)
		{
			Setup();

			while (!token.IsCancellationRequested)
			{
				Send();

				await TaskEx.Delay(1000);
			}
		}

		static void Setup()
		{
			Config = Config.Load(AppDomain.CurrentDomain.BaseDirectory);

			var journals = EventLog.GetEventLogs();

			foreach (var journal in journals)
			{
				journal.EnableRaisingEvents = true;
				journal.EntryWritten += (s, e) => AddLog(journal.Log, e);
			}
		}

		static void AddLog(string journalName, EntryWrittenEventArgs e)
		{
			#pragma warning disable CS0618
			var log = new Log
			{
				MachineName = e.Entry.MachineName,
				JournalName = journalName,
				Category = e.Entry.Category,
				EventId = e.Entry.EventID,
				Message = e.Entry.Message,
				Source = e.Entry.Source,
				TimeGenerated = e.Entry.TimeGenerated,
				Type = e.Entry.EntryType,
				Username = e.Entry.UserName,
			};
			#pragma warning restore CS0618

			lock (Logs)
			{
				Logs.Add(log);
			}

			Console.WriteLine(log.ToConsole());
		}

		static void Send()
		{
			//Console.WriteLine(Logs.Count);
			List<Log> packet;

			lock (Logs)
			{
				packet = Logs.GetRange(0, Math.Min(50, Logs.Count));
			}

			try
			{
				string json = JsonConvert.SerializeObject(new { Logs = packet });

				var req = (HttpWebRequest)WebRequest.Create("http://" + Config.Server + ":" + Config.Port + "/api/agents/reply");

				req.ContentType = "application/json";
				req.Method = "POST";
				req.Timeout = 1000;

				using (var stream =  req.GetRequestStream())
				{
					using (var writer = new StreamWriter(stream))
					{
						writer.Write(json);
					}
				}

				string text;
				using (var response = (HttpWebResponse)req.GetResponse())
				{
					using (var stream = response.GetResponseStream())
					{ 
						using (var reader = new StreamReader(stream))
						{
							text = reader.ReadToEnd();
							reader.Close();
						}
					}
				}

				lock (Logs)
				{
					Logs.RemoveRange(0, packet.Count);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
