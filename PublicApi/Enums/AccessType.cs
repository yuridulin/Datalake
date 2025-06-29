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
	/// Разрешен только просмотр
	/// </summary>
	Viewer = 1,

	/// <summary>
	/// Разрешен только просмотр и запись значений
	/// </summary>
	Editor = 2,

	/// <summary>
	/// Доступ ко всем операциям
	/// </summary>
	Manager = 3,

	/// <summary>
	/// Доступа нет
	/// </summary>
	NoAccess = 4,

	/// <summary>
	/// Полный доступ
	/// </summary>
	Admin = 5,
}
