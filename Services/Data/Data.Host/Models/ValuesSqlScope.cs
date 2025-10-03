using Datalake.Data.Host.Models.Values;

namespace Datalake.Data.Host.Models;

internal class ValuesSqlScope
{
	internal required ValuesTrustedRequest.TimeSettings Settings { get; set; }

	internal required ValuesTrustedRequest[] Requests { get; set; }

	internal required string[] Keys { get; set; }

	internal required int[] TagsId { get; set; }
}

