namespace Datalake.Data.Host.Enums;

/// <summary>
/// Характеристика ответа на запрос на чтение/запись значений по тегу
/// </summary>
public enum ValueResult
{
	/// <summary>
	/// Неизвестная ошибка
	/// </summary>
	UnknownError = 0,

	/// <summary>
	/// Всё хорошо
	/// </summary>
	Ok = 1,

	/// <summary>
	/// Тег не найден
	/// </summary>
	NotFound = 2,

	/// <summary>
	/// Тег уже удален
	/// </summary>
	IsDeleted = 3,

	/// <summary>
	/// Нет доступа
	/// </summary>
	NoAccess = 4,

	/// <summary>
	/// Не мануальный тег (не доступен для записи)
	/// </summary>
	NotManual = 5,

	/// <summary>
	/// Операция успешна, но ответ не получен
	/// </summary>
	ValueNotFound = 6,

	/// <summary>
	/// Ошибка при выполнении действия
	/// </summary>
	InternalError = 7,
}
