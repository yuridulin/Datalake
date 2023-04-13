using System.Net;
using System.Security.Principal;

namespace Logger.Web.Models
{
	public class Controller
	{
		public HttpListenerRequest Request { get; set; } = null;

		public HttpListenerResponse Response { get; set; } = null;

		public WindowsPrincipal User { get; set; } = null;
	}
}
