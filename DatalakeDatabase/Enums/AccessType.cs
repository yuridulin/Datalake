using System.Text.Json.Serialization;

namespace DatalakeDatabase.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccessType
{
	FIRST = -1,
	NOT = 0,
	USER = 1,
	ADMIN = 2,
}
