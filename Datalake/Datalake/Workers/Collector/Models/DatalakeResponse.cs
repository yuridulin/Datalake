using System;

namespace Datalake.Workers.Collector.Models
{
	public class DatalakeResponse
	{
		public DateTime Timestamp { get; set; }

		public InputTag[] Tags { get; set; }
	}
}
