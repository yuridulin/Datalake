using System.Text.Json.Serialization;

namespace DatalakeDb.Enums
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum SourceType
	{
		Inopc = 0,
		Datalake = 1,
	}
}
