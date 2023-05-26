using Logger.Agent.Modules;
using Logger.Library;
using Logger_Agent.Modules;
using Newtonsoft.Json;
using System;
using System.IO;
using System.ServiceProcess;

namespace Logger.Agent
{
	internal class Program
	{
		public static readonly string Version = "1.3";

		static Service Service { get; set; }

		public static AgentGlobalConfig GlobalConfig { get; set; }

		public static AgentConfig Config { get; set; } = new AgentConfig { LastUpdate = DateTime.MinValue };

		static void Main(string[] args)
		{
			if (Environment.UserInteractive)
			{
				Start(args);
				Console.WriteLine("Logger Agent v" + Version + " is active");
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
			Helpers.RaiseEvent(AgentLogSources.Program, "Агент запущен");

			GlobalConfig = JsonConvert.DeserializeObject<AgentGlobalConfig>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "config.json"));

			Events.Start();
			Sender.Start();
			Specs.Start();
			Pings.Start();
			Sql.Start();
			Ntp.Start();
			Syslog.Start();
		}

		public static void Stop()
		{
			Helpers.RaiseEvent(AgentLogSources.Program, "Агент остановлен");

			Ntp.Stop();
			Sql.Stop();
			Pings.Stop();
			Specs.Stop();
			Events.Stop();
			Sender.Stop();
			Syslog.Stop();
		}
	}

	public class Service : ServiceBase
	{
		public Service()
		{
			ServiceName = "Logger Agent";
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
