using Datalake.Database.Enums;

namespace Datalake.Database.Models.Auth;

/// <summary>
/// Вычисленные уровни доступа ко всем защищаемым объектам
/// </summary>
public class UserRights
{
	/// <summary>
	/// Глобальный уровень доступа
	/// </summary>
	public AccessType GlobalAccessType { get; set; }

	/// <summary>
	/// Список всех блоков с указанием доступа к ним
	/// </summary>
	public UserAccessToGroup[] Groups { get; set; } = [];

	/// <summary>
	/// Список всех блоков с указанием доступа к ним
	/// </summary>
	public UserAccessToObject[] Sources { get; set; } = [];

	/// <summary>
	/// Список всех блоков с указанием доступа к ним
	/// </summary>
	public UserAccessToObject[] Blocks { get; set; } = [];

	/// <summary>
	/// Список всех тегов с указанием доступа к ним
	/// </summary>
	public UserAccessToTag[] Tags { get; set; } = [];
}
