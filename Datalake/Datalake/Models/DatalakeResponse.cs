using System;

namespace Datalake.Models
{
	public class DatalakeResponse
	{
		public DateTime Timestamp { get; set; }

		public InputTag[] Tags { get; set; }
	}
}
