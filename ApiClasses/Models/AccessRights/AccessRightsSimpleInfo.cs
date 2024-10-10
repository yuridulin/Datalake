using Datalake.ApiClasses.Enums;

namespace Datalake.ApiClasses.Models.AccessRights;

/// <summary>
/// Общая информация о разрешении
/// </summary>
public class AccessRightsSimpleInfo
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
}
