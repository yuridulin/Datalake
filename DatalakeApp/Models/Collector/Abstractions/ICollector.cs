using DatalakeApp.Models.Receiver;

namespace DatalakeApp.Models.Collector.Abstractions
{
	public interface ICollector
	{
		public int[] Tags { get; set; }

		public Task<ReceiveResponse> Collect();
	}
}
