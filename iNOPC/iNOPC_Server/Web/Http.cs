using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace iNOPC.Server.Web
{
	public class Http
	{
		static HttpListener Listener { get; set; }

		public static List<Session> Sessions = new List<Session>();

		public static string Base { get; set; } = Program.Base + "\\webConsole\\";

		public static DateTime LastUpdate { get; set; } = DateTime.MinValue;

		public static async Task Start()
		{
			Listener = new HttpListener();
			Listener.Prefixes.Add("http://*:" + Program.Configuration.Settings.WebConsolePort + "/");
			Listener.Start();

			while (true)
			{
				var ctx = await Listener.GetContextAsync();
				new Router().Resolve(ctx);
			}
		}

		public static void Stop()
		{
			Listener.Stop();
		}

		public static void Update()
		{
			LastUpdate = DateTime.Now;
		}
	}
}