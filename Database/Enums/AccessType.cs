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
	/// Возможность изменения без наследования
	/// </summary>
	Editor = 10,

	/// <summary>
	/// Возможность изменения с наследованием такого же уровня и к подчиненным объектам
	/// </summary>
	Manager = 50,

	/// <summary>
	/// Полный доступ
	/// </summary>
	Admin = 100,
}
