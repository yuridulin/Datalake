using Datalake.Database.Models.Blocks;
using Datalake.Database.Models.Sources;
using Datalake.Database.Models.Tags;

namespace Datalake.Database.Models.AccessRights;

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
