using Logger.Agent.Modules;
using Logger.Library;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Logger_Agent.Modules
{
	public class Syslog
	{
		static UdpClient Udp { get; set; }

		static Thread Thread { get; set; }

		static bool IsWork { get; set; } = false;

		public static void Start()
		{
			IsWork = true;
			Thread = new Thread(() => 
			{
				Udp = new UdpClient(514, AddressFamily.InterNetwork);

				while (IsWork)
				{
					try
					{
						var remote = new IPEndPoint(0, 0);
						var result = Udp.Receive(ref remote);
						if (result == null || result.Length == 0) return;

						var message = Encoding.UTF8.GetString(result);
						var parts = message.Split(' ');
						
						int priority = int.TryParse(parts[0].Substring(1, parts[0].IndexOf('>') - 1), out int i) ? i : 0;
						int typeId = priority % 8;
						int categoryId = (priority - typeId) / 8;

						var log = new AgentLog
						{
							Journal = "Syslog",
							Endpoint = parts[2],
							Source = parts[3],
							EventId = int.TryParse(parts[4], out i) ? i : 0,
							Message = string.Join(" ", parts.Skip(7).ToArray()).Trim(),
							TimeStamp = DateTime.TryParse(parts[1], out DateTime d) ? d : DateTime.Now,
							Category = Category(categoryId),
							Type = Type(typeId)
						};

						Events.FilterAndAddLog(log);
					}
					catch { }
				}
			});
			Thread.Start();
		}

		public static void Stop()
		{
			try
			{
				IsWork = false;
				Udp.Close();
				Thread.Abort();
			}
			catch { }
			Helpers.RaiseEvent(AgentLogSources.Syslog, "stopped");
		}

		static string Category(int categoryId)
		{
			if (categoryId > 11) return "Unknown";

			var categories = new[] {
				"Kernel",
				"User",
				"Mail",
				"System",
				"Security",
				"Syslog",
				"Printer",
				"Network",
				"UUCP",
				"Clock",
				"Security",
				"FTP",
			};

			return categories[categoryId - 12];
		}

		static string Type(int typeId)
		{
			if (typeId < 4) return "Error";
			else if (typeId == 4) return "Warning";
			else return "Information";
		}
	}
}
