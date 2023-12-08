using Datalake.Database;
using Datalake.Enums;
using System.Net;

namespace Datalake.Web.Models
{
	public class Controller
	{
		public HttpListenerRequest Request { get; set; } = null;

		public HttpListenerResponse Response { get; set; } = null;

		public User User { get; set; } = new User { Name = string.Empty, AccessType = AccessType.NOT };

		public Result Data(object data) => new Result { Data = data };

		public Result Done(string data) => new Result { Done = data };

		public Result Error(string data) => new Result { Error = data };
	}
}
