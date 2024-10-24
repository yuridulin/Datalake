using Datalake.Database.Exceptions.Base;

namespace Datalake.Database.Exceptions;

/// <summary>
/// Ошибка о том, что предоставленные данные отсутствуют, неверно указаны или неправильно предоставлены
/// </summary>
/// <param name="message">Сообщение после строки "Неверное значение: "</param>
public class InvalidValueException(string? message) : DatalakeException(message)
{
	/// <summary>
	/// Генерация строкового сообщения о ошибке
	/// </summary>
	/// <returns>Сообщение об ошибке</returns>
	public override string ToString()
	{
		return "Неверное значение: " + Message;
	}
}
