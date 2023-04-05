using System;

namespace Datalake.Collector.Models
{
	public class DatalakeResponse
	{
		public DateTime Timestamp { get; set; }

		public InopcTag[] Tags { get; set; }
	}
}
