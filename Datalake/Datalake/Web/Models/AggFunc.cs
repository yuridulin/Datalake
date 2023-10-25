using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Datalake.Web.Models
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum AggFunc
	{
		List,
		Sum,
		Avg,
		Min,
		Max,
	}
}