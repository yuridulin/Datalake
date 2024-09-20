using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions.Base;

namespace Datalake.ApiClasses.Exceptions;

/// <summary>
/// Ошибка при выполнении какой-либо операции с базой данных, связанная с выполнением запросов
/// </summary>
/// <param name="message">Сообщение после "Ошибка базы данных: "</param>
/// <param name="innerException">Реальная ошибка, возникшая при работе с БД</param>
public class DatabaseException(string? message, Exception? innerException) : DatalakeException(message, innerException)
{
	/// <summary>
	/// Генерация строкового сообщения о ошибке
	/// </summary>
	/// <returns>Сообщение об ошибке</returns>
	public override string ToString()
	{
		return "Ошибка базы данных: " + Message;
	}

	/// <summary>
	/// Ошибка при выполнении какой-либо операции с базой данных, связанная с выполнением запросов
	/// </summary>
	/// <param name="message">Сообщение после "Ошибка базы данных: "</param>
	/// <param name="standartError">Тип сообщения, которое будет добавлено как внутренная ошибка к создаваемой ошибке базы данных</param>
	public DatabaseException(string? message, DatabaseStandartError standartError)
		: this(message, standartError switch
		{
			DatabaseStandartError.IdIsNull => IdIsNull,
			DatabaseStandartError.UpdatedZero => CountUpdatedRowsIsZero,
			DatabaseStandartError.DeletedZero => CountDeletedRowsIsZero,
			_ => Unknown,
		})
	{ }

	static readonly Exception IdIsNull = new("Идентификатор новой записи равен NULL");

	static readonly Exception CountUpdatedRowsIsZero = new("Количество измененных строк равно 0");

	static readonly Exception CountDeletedRowsIsZero = new("Количество удаленных строк равно 0");

	static readonly Exception Unknown = new("Стандартная ошибка, описание которой не определено");
}
