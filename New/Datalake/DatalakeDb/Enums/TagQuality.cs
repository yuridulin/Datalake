using System.Text.Json.Serialization;

namespace DatalakeDb.Enums
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum TagQuality
	{
		Bad = 0,
		Bad_NoConnect = 4,
		Bad_NoValues = 8,
		Bad_ManualWrite = 26,
		Good = 192,
		Good_ManualWrite = 216,
		Unknown = -1,
	}
}
