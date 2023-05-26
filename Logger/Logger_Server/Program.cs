using LinqToDB.Data;
using Logger.Workers;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Logger
{
	internal class Program
	{
		static CancellationTokenSource TokenSource { get; set; }

		static Service Service { get; set; }

		static void Main(string[] args)
		{
			if (Environment.UserInteractive)
			{
				Start(args);
				Console.WriteLine("Logger Server is active");
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

		public static void Start(string[] args)
		{
			TokenSource = new CancellationTokenSource();

			Task.Run(() => DeployWorker.Work(TokenSource.Token));
			Task.Run(() => CacheWorker.Work(TokenSource.Token));
			Task.Run(() => ChannelsWorker.Work(TokenSource.Token));
			Task.Run(() => ReceiveWorker.Work(TokenSource.Token));
			//Task.Run(() => TelegramWorker.Work(TokenSource.Token));
			Task.Run(() => NetworkWorker.Work(TokenSource.Token));

			#if DEBUG
			DataConnection.TurnTraceSwitchOn();
			DataConnection.WriteTraceLine = (s, s1, s2) => Debug.WriteLine(s + " | " + s1 + " | " + s2);
			#endif
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
			ServiceName = "Logger Server";
		}

		protected override void OnStart(string[] args)
		{
			Program.Start(args);
		}

		protected override void OnStop()
		{
			Program.Stop();
		}
	}
}
