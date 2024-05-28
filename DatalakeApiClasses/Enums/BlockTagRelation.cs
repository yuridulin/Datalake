using System.Text.Json.Serialization;

namespace DatalakeApiClasses.Enums
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum BlockTagRelation
	{
		Static = 0,
		Input = 1,
		Output = 2,
	}
}
