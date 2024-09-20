using Datalake.ApiClasses.Enums;
using Datalake.Database.Models;

namespace Datalake.Database.Extensions;

public static class TagHistoryExtension
{
	public static object? GetTypedValue(this TagHistory tagHistory, TagType type) => type switch
	{
		TagType.String => tagHistory.Text,
		TagType.Number => tagHistory.Number,
		TagType.Boolean => tagHistory.Number.HasValue ? tagHistory.Number != 0 : null,
		_ => null,
	};
}
