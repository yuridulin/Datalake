using System.Text.Json.Serialization;

namespace DatalakeApiClasses.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogCategory
{
	Core = 0,
	Database = 10,
	Collector = 20,
	Api = 30,
	Calc = 40,
	Source = 50,
	Tag = 60,
	Http = 70,
	Users = 80,
}
