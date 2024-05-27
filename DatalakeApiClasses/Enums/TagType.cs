using System.Text.Json.Serialization;

namespace DatalakeApiClasses.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TagType
{
	String = 0,
	Number = 1,
	Boolean = 2,
}
