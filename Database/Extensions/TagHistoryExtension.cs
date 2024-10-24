using Datalake.Database.Enums;
using Datalake.Database.Tables;

namespace Datalake.Database.Extensions;

internal static class TagHistoryExtension
{
	internal static object? GetTypedValue(this TagHistory tagHistory, TagType type) => type switch
	{
		TagType.String => tagHistory.Text,
		TagType.Number => tagHistory.Number,
		TagType.Boolean => tagHistory.Number.HasValue ? tagHistory.Number != 0 : null,
		_ => null,
	};
}
