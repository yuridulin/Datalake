using Datalake.ApiClasses.Models.Blocks;
using Datalake.ApiClasses.Models.Sources;
using Datalake.ApiClasses.Models.Tags;

namespace Datalake.ApiClasses.Models.AccessRights;

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
