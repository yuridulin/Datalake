using Datalake.PublicApi.Enums;

namespace Datalake.Inventory.Constants;

/// <summary>
/// Константы-списки
/// </summary>
public class Lists
{
	/// <summary>
	/// Не настраиваемые источники данных
	/// </summary>
	public static readonly SourceType[] CustomSources = [
		SourceType.System,
		SourceType.Calculated,
		SourceType.Manual,
		SourceType.Aggregated,
		SourceType.NotSet
	];

	/// <summary>
	/// Запросы на чтение тегов, которые не являются запросами для просмотра в веб-консоли
	/// </summary>
	public static readonly HashSet<string> InnerRequests = [
		"block-values", "tag-current-value", "tags-table"
	];
}
