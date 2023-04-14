using Logger.Database;
using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Logger
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
			TokenSource = new CancellationTokenSource();

			using (var db = new DatabaseContext())
			{
				db.Setup();
			}

			Task.Run(() => Web.Http.Start());
			Task.Run(() => Logs.LogsWorker.Start(TokenSource.Token));
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
			ServiceName = nameof(Logger);
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
