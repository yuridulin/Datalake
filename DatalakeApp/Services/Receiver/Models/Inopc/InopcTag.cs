using DatalakeApp.Services.Receiver.Models.Inopc.Enums;

namespace DatalakeApp.Services.Receiver.Models.Inopc;

public class InopcTag
{
	public object? Value { get; set; } = null;

	public InopcTagQuality Quality { get; set; } = 0;

	public string Name { get; set; } = "";

	public uint TagHandle { get; set; } = 0;

	public InopcTagType Type { get; set; }
}
