using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Datalake.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum AggFunc
	{
		List = 0,
		Sum = 1,
		Avg = 2,
		Min = 3,
		Max = 4,
	}
}