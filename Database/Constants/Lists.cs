using Datalake.PublicApi.Enums;

namespace Datalake.Database.Constants;

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
}
