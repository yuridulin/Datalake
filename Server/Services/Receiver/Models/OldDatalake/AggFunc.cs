using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Datalake.Server.Services.Receiver.Models.OldDatalake;

[JsonConverter(typeof(StringEnumConverter))]
internal enum AggFunc
{
	List = 0,
	Sum = 1,
	Avg = 2,
	Min = 3,
	Max = 4,
}
