using System.Text;

namespace Logger_Server.Http
{
	internal class HttpResponse
	{
		public int StatusCode { get; set; } = 501;

		public string ContentType { get; set; } = "text/html";

		public byte[] Bytes { get; set; } = new byte[ 0 ];

		public string String
		{
			set
			{
				Bytes = Encoding.UTF8.GetBytes(value);
			}
		}
	}
}
