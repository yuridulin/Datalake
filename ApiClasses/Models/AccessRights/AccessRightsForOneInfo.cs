using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.Blocks;
using Datalake.ApiClasses.Models.Sources;
using Datalake.ApiClasses.Models.Tags;

namespace Datalake.ApiClasses.Models.AccessRights;

/// <summary>
/// Информация о разрешении
/// </summary>
public class AccessRightsForOneInfo
{
	/// <summary>
	/// Идентификатор разрешения
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Тип доступа
	/// </summary>
	public AccessType AccessType { get; set; }

	/// <summary>
	/// Является ли разрешение глобальным
	/// </summary>
	public required bool IsGlobal { get; set; }

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
