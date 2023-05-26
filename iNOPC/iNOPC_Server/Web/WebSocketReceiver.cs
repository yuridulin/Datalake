using WebSocketSharp;
using WebSocketSharp.Server;

namespace iNOPC.Server.Web
{
	public class WebSocketReceiver : WebSocketBehavior
	{
		protected override void OnMessage(MessageEventArgs e)
		{
			// Делать какие-нибудь действия
		}
	}
}