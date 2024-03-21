using System.Text.Json.Serialization;

namespace DatalakeDatabase.Enums
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum SourceType
	{
		Custom = -1,
		Inopc = 0,
		Datalake = 1,
	}
}
