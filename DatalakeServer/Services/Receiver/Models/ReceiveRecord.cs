using DatalakeApiClasses.Enums;

namespace DatalakeServer.Services.Receiver.Models;

public class ReceiveRecord
{
	public string Name { get; set; } = "";

	public object? Value { get; set; } = null;

	public TagType Type { get; set; }

	public TagQuality Quality { get; set; } = 0;
}
