namespace Datalake.ApiClasses.Enums;

/// <summary>
/// Тип сообщения, которое будет добавлено как внутренная ошибка к создаваемой ошибке базы данных
/// </summary>
public enum DatabaseStandartError
{
	/// <summary>
	/// Идентификатор новой записи равен NULL
	/// </summary>
	IdIsNull,

	/// <summary>
	/// Количество измененных строк равно 0
	/// </summary>
	UpdatedZero,

	/// <summary>
	/// Количество удаленных строк равно 0
	/// </summary>
	DeletedZero,
}
