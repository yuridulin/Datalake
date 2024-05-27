namespace DatalakeServer.Services.Receiver.Models.Inopc;

public class InopcResponse
{
	public DateTime Timestamp { get; set; }

	public required InopcTag[] Tags { get; set; }
}
