using iNOPC.Server.Storage;
using System;

namespace iNOPC.Server.Web.RequestTypes
{
	public class DatalakeResponse
	{
		public DateTime Timestamp { get; set; }

		public Tag[] Tags { get; set; }
	}
}
