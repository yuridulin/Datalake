namespace Datalake.Data.Application.Models.Values;

internal class ValuesSqlScope
{
	internal required TimeSettings Settings { get; set; }

	internal required ValuesTrustedRequest[] Requests { get; set; }

	internal required string[] Keys { get; set; }

	internal required int[] TagsId { get; set; }
}
