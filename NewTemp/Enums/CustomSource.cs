using System.Text.Json.Serialization;

namespace DatalakeDatabase.Enums
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum CustomSource
	{
		System = 0,
		Calculated = -1,
		Manual = -2
	}
}
