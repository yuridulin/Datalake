using DatalakeDatabase.Enums;
using DatalakeDatabase.Models;

namespace DatalakeDatabase.Extensions;

internal static class TagHistoryExtension
{
	public static object? GetValue(this TagHistory tagHistory, TagType type) => type switch
	{
		TagType.String => tagHistory.Text,
		TagType.Number => tagHistory.Number,
		TagType.Boolean => tagHistory.Number.HasValue ? tagHistory.Number != 0 : null,
		_ => null,
	};
}
