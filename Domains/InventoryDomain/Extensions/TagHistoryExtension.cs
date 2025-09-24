using Datalake.PublicApi.Abstractions;
using Datalake.PublicApi.Enums;

internal static class TagHistoryExtension
{
	internal static object? GetTypedValue(this IHistory history, TagType type) => type switch
	{
		TagType.String => history.Text,
		TagType.Number => history.Number,
		TagType.Boolean => history.Number.HasValue ? history.Number != 0 : null,
		_ => null,
	};
}
