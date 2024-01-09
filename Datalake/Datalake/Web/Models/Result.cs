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
			if (Data != null && string.IsNullOrEmpty(Done) && string.IsNullOrEmpty(Warning) && string.IsNullOrEmpty(Error))
			{
				return JsonConvert.SerializeObject(Data);
			}
			else if (!string.IsNullOrEmpty(Done)) return JsonConvert.SerializeObject(new { Done, Data });
			else if (!string.IsNullOrEmpty(Warning)) return JsonConvert.SerializeObject(new { Warning, Data });
			else if (!string.IsNullOrEmpty(Error)) return JsonConvert.SerializeObject(new { Error, Data });
			else return JsonConvert.SerializeObject(new { Error = "Unknown reason", Data });
		}
	}
}
