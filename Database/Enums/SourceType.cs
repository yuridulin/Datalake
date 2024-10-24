namespace Datalake.Database.Enums;

/// <summary>
/// Тип получения данных с источника
/// </summary>
public enum SourceType
{
	/// <summary>
	/// Не определен или не известен
	/// </summary>
	Unknown = -100,

	/// <summary>
	/// Уникальный
	/// </summary>
	Custom = -1,

	/// <summary>
	/// INOPC-сервер
	/// </summary>
	Inopc = 0,

	/// <summary>
	/// Другая нода базы данных Datalake
	/// </summary>
	Datalake = 1,

	/// <summary>
	/// Обновленный на .NET Core Datalake, версия 1
	/// </summary>
	DatalakeCore_v1 = 2,
}
