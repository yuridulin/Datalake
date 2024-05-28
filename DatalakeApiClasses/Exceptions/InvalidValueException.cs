using DatalakeApiClasses.Exceptions.Base;

namespace DatalakeApiClasses.Exceptions;

/// <summary>
/// Ошибка о том, что предоставленные данные отсутствуют, неверно указаны или неправильно предоставлены
/// </summary>
/// <param name="message">Сообщение после строки "Неверное значение: "</param>
public class InvalidValueException(string? message) : DatalakeException(message)
{
	public override string ToString()
	{
		return "Неверное значение: " + Message;
	}
}
