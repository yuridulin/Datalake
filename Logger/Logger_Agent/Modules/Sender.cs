using Logger.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Timers;

namespace Logger.Agent.Modules
{
	public static class Sender
	{
		public static void Start()
		{
			Timer = new Timer(Program.GlobalConfig.ReplyIntervalSeconds * 1000);
			Timer.Elapsed += Timer_Elapsed;
			Timer.Start();
		}

		public static void Stop()
		{
			Timer.Stop();
			Helpers.RaiseEvent(AgentLogSources.Sender, "stopped");
		}

		public static void AddLog(AgentLog log)
		{
			lock (LogQuery)
			{
				while (LogQuery.Count >= 1000) LogQuery.RemoveAt(0);
				LogQuery.Add(log);
			}
		}

		public static void AddSpecs(List<AgentSpec> specs)
		{
			lock (SpecQuery)
			{
				SpecQuery.AddRange(specs.ToList());
			}
		}

		// реализация

		static Timer Timer { get; set; }

		static List<AgentLog> LogQuery { get; set; } = new List<AgentLog>();

		static List<AgentSpec> SpecQuery { get; set; } = new List<AgentSpec>();

		static void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				var reply = new AgentReply
				{
					Endpoint = Environment.MachineName.ToUpper(),
					LastUpdate = Program.Config.LastUpdate,
					Version = Program.Version,
					Logs = LogQuery.ToList(),
					Specs = SpecQuery.ToList(),
				};

				var req = (HttpWebRequest)WebRequest.Create("http://" + Program.GlobalConfig.Server + ":" + Program.GlobalConfig.Port + "/");

				req.ContentType = "application/json";
				req.Method = "POST";
				req.Timeout = 5000;

				string json = JsonConvert.SerializeObject(reply);

				using (var streamWriter = new StreamWriter(req.GetRequestStream()))
				{
					streamWriter.Write(json);
				}

				string text;
				var res = (HttpWebResponse)req.GetResponse();
				using (var streamReader = new StreamReader(res.GetResponseStream()))
				{
					text = streamReader.ReadToEnd();
				}

				Helpers.RaiseEvent(AgentLogSources.Sender, reply.LastUpdate + " | " + reply.Logs.Count + "L " + reply.Specs.Count + "S >> " + res.StatusCode);

				var agentConfig = JsonConvert.DeserializeObject<AgentConfig>(text);

				Helpers.RaiseEvent(AgentLogSources.Sender, reply.LastUpdate + " << " + agentConfig.LastUpdate + " | " + agentConfig.Filters.Count + "F " + agentConfig.Pings.Count + "P " + agentConfig.SqlActions.Count + "S");
				if (agentConfig.LastUpdate > reply.LastUpdate)
				{
					Helpers.RaiseEvent(AgentLogSources.Sender, "Config updated");
					Program.Config = agentConfig;
				}

				LogQuery.Clear();
				SpecQuery.Clear();
			}
			catch (Exception ex)
			{
				Helpers.RaiseEvent(AgentLogSources.Sender, "Ошибка\r\n" + ex.Message + "\r\n" + ex.StackTrace, true);
			}
		}
	}
}
