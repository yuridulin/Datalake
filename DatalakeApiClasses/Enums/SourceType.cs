using System.Text.Json.Serialization;

namespace DatalakeApiClasses.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SourceType
{
	Unknown = -100,
	Custom = -1,
	Inopc = 0,
	Datalake = 1,
}
