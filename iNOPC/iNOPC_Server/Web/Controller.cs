using iNOPC.Server.Models.Configurations;
using System.Net;
using System.Security.Principal;

namespace iNOPC.Server.Web
{
	public class Controller
	{
		public HttpListenerRequest Request { get; set; }

		public HttpListenerResponse Response { get; set; }

		public WindowsPrincipal User { get; set; }

		public AccessType AccessedType { get; set; }
	}
}
