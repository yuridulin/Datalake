using System.Text.Json.Serialization;

namespace DatalakeApiClasses.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogType
{
	Trace = 0,
	Information = 1,
	Success = 2,
	Warning = 3,
	Error = 4,
}
