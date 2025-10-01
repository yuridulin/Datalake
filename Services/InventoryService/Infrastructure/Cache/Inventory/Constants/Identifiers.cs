using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Infrastructure.Cache.Inventory.Constants;

/// <summary>
/// Константы, обозначающие конкретные значения
/// </summary>
public static class Identifiers
{
	/// <summary>
	/// Идентификатор источника, которого не существует
	/// </summary>
	public static int UnsetSource { get; } = (int)SourceType.NotSet - 1;
}
