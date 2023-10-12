using Datalake.Workers.Logs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers.Logs
{
	public static class LogsWorker
	{
		static List<Log> Logs { get; set; } = new List<Log>();

		public static async Task Start(CancellationToken token)
		{
			Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Logs");

			while (!token.IsCancellationRequested)
			{
				Write();

				await Task.Delay(5000);
			}
		}

		static string Path() => AppDomain.CurrentDomain.BaseDirectory + "Logs\\" + DateTime.Today.ToString("yyyy_MM_dd") + ".txt";

		static void Write()
		{
			lock (Logs)
			{
				File.AppendAllLines(Path(), Logs.Select(x => x.ToText()));
				Logs.Clear();
				Console.Clear();
			}
		}

		public static void Add(string module, string message, LogType type)
		{
			var log = new Log
			{
				Date = DateTime.Now,
				Module = module,
				Message = message,
				Type = type,
			};

			lock (Logs)
			{
				Logs.Add(log);
			}

			Console.WriteLine(log.ToText());
		}

		public static List<Log> Read()
		{
			return File.ReadAllLines(Path())
				.Select(x => Log.FromText(x))
				.ToList();
		}
	}
}
