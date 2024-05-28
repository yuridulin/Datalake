namespace DatalakeServer.Services.Receiver.Models;

public class ReceiveResponse
{
	public DateTime Timestamp { get; set; }

	public ReceiveRecord[] Tags { get; set; } = [];
}
