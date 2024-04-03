namespace DatalakeApp.Models.Receiver
{
	public class ReceiveResponse
	{
		public DateTime Timestamp { get; set; }

		public ReceiveRecord[] Tags { get; set; } = [];
	}
}
