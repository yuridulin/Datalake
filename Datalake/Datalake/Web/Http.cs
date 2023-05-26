using System.Net;
using System.Threading.Tasks;

namespace Datalake.Web
{
	public class Http
	{
		static HttpListener Listener { get; set; }

		public static async Task Start()
		{
			Listener = new HttpListener();
			Listener.Prefixes.Add("http://*:83/");
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
	}
}