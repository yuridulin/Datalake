using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Inventory.Api.Models.Sources;
using Datalake.Inventory.Api.Models.Tags;

namespace Datalake.Inventory.Api.Models.AccessRules;

/// <summary>
/// Информация о разрешении субьекта на доступ к объекту
/// </summary>
public class AccessRightsForOneInfo : AccessRightsSimpleInfo
{
	/// <summary>
	/// Тег, на который выдано разрешение
	/// </summary>
	public TagSimpleInfo? Tag { get; set; } = null;

	/// <summary>
	/// Блок, на который выдано разрешение
	/// </summary>
	public BlockSimpleInfo? Block { get; set; } = null;

	/// <summary>
	/// Источник, на который выдано разрешение
	/// </summary>
	public SourceSimpleInfo? Source { get; set; } = null;
}
