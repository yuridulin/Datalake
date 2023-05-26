using Datalake.Database;
using LinqToDB.Data;
using System;
using System.Diagnostics;
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
				Console.WriteLine(nameof(Datalake) + " работает.");
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
			#if DEBUG
			DataConnection.TurnTraceSwitchOn();
			DataConnection.WriteTraceLine = (s, s1, s2) => Debug.WriteLine(s + " | " + s1 + " | " + s2);
			#endif

			TokenSource = new CancellationTokenSource();

			using (var db = new DatabaseContext()) db.Recreate();
			Task.Run(() => Web.Http.Start());
			Task.Run(() => Collector.CollectorWorker.Start(TokenSource.Token));
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
