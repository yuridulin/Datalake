using Datalake.Web.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Datalake.Web
{
	public class Server
	{
		static HttpListener Listener { get; set; }

		public static List<UserSession> Sessions { get; set; } = new List<UserSession>();

		public static async Task Start()
		{
			Listener = new HttpListener();
			Listener.Prefixes.Add("http://*:83/");
			Listener.Start();

			while (true)
			{
				var ctx = await Listener.GetContextAsync();
#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до тех пор, пока вызов не будет завершен
				Task.Run(() => new Router().Resolve(ctx));
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до тех пор, пока вызов не будет завершен
			}
		}

		public static void Stop()
		{
			Listener.Stop();
		}
	}
}