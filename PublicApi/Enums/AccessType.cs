namespace Datalake.PublicApi.Enums;

/// <summary>
/// Уровень доступа
/// </summary>
public enum AccessType : byte
{
	/// <summary>
	/// Правило не применяется
	/// </summary>
	NotSet = 0,

	/// <summary>
	/// Доступа нет
	/// </summary>
	NoAccess = 1,

	/// <summary>
	/// Разрешен только просмотр
	/// </summary>
	Viewer = 2,

	/// <summary>
	/// Возможность изменения без наследования
	/// </summary>
	Editor = 3,

	/// <summary>
	/// Возможность изменения с наследованием такого же уровня и к подчиненным объектам
	/// </summary>
	Manager = 4,

	/// <summary>
	/// Полный доступ
	/// </summary>
	Admin = 5,
}
