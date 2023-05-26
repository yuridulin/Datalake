using System;
using System.Diagnostics;

namespace Logger.Library
{
	public static class Helpers
	{
		static string AgentLog = "Logger Agent";

		static string ServerLog = "Logger Server";

		public static void RaiseEvent(string source, string message, bool isError = false)
		{
			Write(AgentLog, source, message, isError);
		}

		public static void RaiseServerEvent(string source, string message, bool isError = false)
		{
			Write(ServerLog, source, message, isError);
		}

		static void Write(string logName, string source, string message, bool error)
		{
			try
			{
				var oldLogName = EventLog.LogNameFromSourceName(source, ".");

				if (oldLogName != logName)
				{
					EventLog.DeleteEventSource(source);
					EventLog.CreateEventSource(source, logName);
				}
			}
			catch { }

			try
			{
				using (var log = new EventLog(logName))
				{
					log.Source = source;
					log.WriteEntry(message, error ? EventLogEntryType.Error : EventLogEntryType.Information);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				Console.WriteLine((error ? "[ERR]" : "[INF]") + " " + source + " => " + message);
			}
		}
	}

	public static class AgentLogSources
	{
		public static readonly string Program = "LoggerAgentProgram";
		public static readonly string Sender = "LoggerAgentSender";
		public static readonly string Specs = "LoggerAgentSpecs";
		public static readonly string Ping = "LoggerAgentPing";
		public static readonly string Sql = "LoggerAgentSql";
		public static readonly string Ntp = "LoggerAgentNtp";
		public static readonly string Syslog = "LoggerAgentSyslog";
	}

	public static class ServerLogSources
	{
		public static readonly string Program = "LoggerServerProgram";
		public static readonly string Cache = "LoggerServerCache";
		public static readonly string Channels = "LoggerServerChannels";
		public static readonly string Deploy = "LoggerServerDeploy";
		public static readonly string Network = "LoggerServerNetwork";
		public static readonly string Receive = "LoggerServerReceive";
		public static readonly string Telegram = "LoggerServerTelegram";
	}
}
