namespace Datalake.Database.Enums;

/// <summary>
/// Уровень доступа
/// </summary>
public enum AccessType
{
	/// <summary>
	/// Правило не применяется
	/// </summary>
	NotSet = -100,

	/// <summary>
	/// Доступа нет
	/// </summary>
	NoAccess = 0,

	/// <summary>
	/// Разрешен только просмотр
	/// </summary>
	Viewer = 5,

	/// <summary>
	/// Доступ к действиям
	/// </summary>
	User = 10,

	/// <summary>
	/// Полный доступ
	/// </summary>
	Admin = 100,
}
