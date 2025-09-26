using Datalake.PublicApi.Enums;

namespace Datalake.Inventory.Constants;

/// <summary>
/// Константы, обозначающие конкретные значения
/// </summary>
public static class Identifiers
{
	/// <summary>
	/// Идентификатор источника, которого не существует
	/// </summary>
	public static readonly int UnsetSource = (int)SourceType.NotSet - 1;
}
