using System.Text.Json.Serialization;

namespace DatalakeDatabase.Enums
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum BlockTagRelation
	{
		Static = 0,
		Input = 1,
		Output = 2,
	}
}
