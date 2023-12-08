using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Datalake.Web.Models
{
	internal class Answer
	{
		public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.NotImplemented;

		public string ContentType { get; set; } = "text/html";

		public byte[] Bytes { get; set; } = new byte[0];

		public string String
		{
			set
			{
				Bytes = Encoding.UTF8.GetBytes(value);
			}
		}
	}
}
