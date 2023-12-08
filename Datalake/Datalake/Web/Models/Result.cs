using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace Datalake.Web.Models
{
	public class Result
	{
		public string Done { get; set; } = null;

		public string Warning { get; set; } = null;

		public string Error { get; set; } = null;

		public object Data { get; set; } = null;

		public HttpStatusCode StatusCode =>
			!string.IsNullOrEmpty(Error) ? HttpStatusCode.InternalServerError
			: !string.IsNullOrEmpty(Warning) ? HttpStatusCode.InternalServerError
			: HttpStatusCode.OK;

		public Dictionary<string, string> Headers { get; internal set; }

		public string ToJson()
		{
			if (Data == null)
			{
				if (!string.IsNullOrEmpty(Done)) return JsonConvert.SerializeObject(new { Done });
				else if (!string.IsNullOrEmpty(Warning)) return JsonConvert.SerializeObject(new { Warning });
				else if (!string.IsNullOrEmpty(Error)) return JsonConvert.SerializeObject(new { Error });
				else return JsonConvert.SerializeObject(new { Error = "Unknown reason " });
			}
			else
			{
				return JsonConvert.SerializeObject(Data);
			}
		}
	}
}
