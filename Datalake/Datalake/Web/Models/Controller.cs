using System.Net;
using System.Security.Principal;

namespace Datalake.Web.Models
{
	public class Controller
	{
		public HttpListenerRequest Request { get; set; } = null;

		public HttpListenerResponse Response { get; set; } = null;

		public WindowsPrincipal User { get; set; } = null;

		public Result Data(object data) => new Result { Data = data };

		public Result Done(string data) => new Result { Done = data };

		public Result Warning(string data) => new Result { Warning = data };

		public Result Error(string data) => new Result { Error = data };
	}
}
