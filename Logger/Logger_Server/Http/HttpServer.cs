using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Logger_Server.Http
{
	public static class HttpServer
	{
		static HttpListener Listener { get; set; }

		public static async Task Start(CancellationToken token)
		{
			Listener = new HttpListener();
			Listener.Prefixes.Add("http://*:4330/");
			Listener.Start();

			while (!token.IsCancellationRequested)
			{
				var ctx = await Listener.GetContextAsync();
				new HttpRouter().Resolve(ctx);
			}
		}
	}
}
