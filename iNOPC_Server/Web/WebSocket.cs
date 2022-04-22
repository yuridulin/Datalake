using Newtonsoft.Json;
using WebSocketSharp.Server;

namespace iNOPC.Server.Web
{
	public class WebSocket
    {
        private static WebSocketServer Server { get; set; }

        public static void Start()
        {
            Server = new WebSocketServer(Program.Configuration.Settings.WebConsoleSocketPort);
            Server.AddWebSocketService<WebSocketReceiver>("/");
            Server.Start();
        }

        public static void Stop() => Server.Stop();

        public static void Broadcast(string method) => Server.WebSocketServices.Broadcast(method);

        public static void Log(long id, object log)
        {
            Server.WebSocketServices.Broadcast("device.logs:" + id + "|" + JsonConvert.SerializeObject(log));
        }
    }
}