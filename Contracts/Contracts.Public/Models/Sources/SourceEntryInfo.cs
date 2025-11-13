namespace Datalake.Contracts.Public.Models.Sources;

/// <summary>
/// Информация о сопоставлении данных в источнике и в базе
/// </summary>
public class SourceEntryInfo
{
	/// <summary>
	/// Сопоставленная запись в источнике
	/// </summary>
	public SourceItemInfo? ItemInfo { get; set; }

	/// <summary>
	/// Сопоставленный тег в базе
	/// </summary>
	public SourceTagInfo? TagInfo { get; set; }

	/// <summary>
	/// Используется ли тег в запросах
	/// </summary>
	public DateTime? IsTagInUse { get; set; }
}
