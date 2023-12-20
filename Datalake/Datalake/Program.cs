using Datalake.Database;
using Datalake.Models;
using Datalake.Workers;
using LinqToDB.Common;
using LinqToDB.Data;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake
{
	internal class Program
	{
		static CancellationTokenSource TokenSource { get; set; }

		static Service Service { get; set; }

		static void Main()
		{
			if (Environment.UserInteractive)
			{
				Start();
				Console.ReadLine();
				Stop();
			}
			else
			{
				using (Service = new Service())
				{
					ServiceBase.Run(Service);
				}
			}
		}

		public static void Start()
		{
			try
			{
				var startupOptions = JsonConvert.DeserializeObject<StartupOptions>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "startupOptions.json"));

				#if DEBUG
				DatabaseContext.ConnectionSetup = startupOptions.ConnectionStrings["Debug"];
				DataConnection.TurnTraceSwitchOn();
				DataConnection.WriteTraceLine = (s, s1, s2) => Debug.WriteLine(s + " | " + s1 + " | " + s2);
				//Configuration.Linq.GenerateExpressionTest = true;
				#else
				DatabaseContext.ConnectionSetup = startupOptions.ConnectionStrings["Release"];
				#endif

				Web.Server.Port = startupOptions.WebServerPort;
				File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Content\\startup.js", $"const PORT = {startupOptions.WebServerPort};");
			}
			catch (Exception ex)
			{
				File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "startupError.txt", $"{ex.Message}\n{ex.Source}\n{ex.StackTrace}");
				throw ex;
			}

			JsonConvert.DefaultSettings = () => new JsonSerializerSettings
			{
				DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
			};

			Configuration.Linq.GuardGrouping = false; // чтение последних значений для записи Initial при создании таблицы
			TokenSource = new CancellationTokenSource();

			using (var db = new DatabaseContext()) db.Migrate();
			WorkersList.Start(TokenSource.Token);
			Task.Run(() => Web.Server.Start());
		}

		public static void Stop()
		{
			TokenSource.Cancel();
		}
	}

	public class Service : ServiceBase
	{
		public Service()
		{
			ServiceName = nameof(Datalake);
		}

		protected override void OnStart(string[] args)
		{
			Program.Start();
		}

		protected override void OnStop()
		{
			Program.Stop();
		}
	}
}
