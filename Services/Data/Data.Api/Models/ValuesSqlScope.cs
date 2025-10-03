using Datalake.Data.Api.Models.Values;

namespace Datalake.Data.Api.Models;

internal class ValuesSqlScope
{
	internal required ValuesTrustedRequest.TimeSettings Settings { get; set; }

	internal required ValuesTrustedRequest[] Requests { get; set; }

	internal required string[] Keys { get; set; }

	internal required int[] TagsId { get; set; }
}

