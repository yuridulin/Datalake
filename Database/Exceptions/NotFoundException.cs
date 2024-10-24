using Datalake.Database.Exceptions.Base;

namespace Datalake.Database.Exceptions;

/// <summary>
/// Ошибка при получении каких-либо данных о том, что эти данные не были найдены
/// </summary>
/// <param name="message">Сообщение после "Не найдено: "</param>
public class NotFoundException(string? message) : DatalakeException(message)
{
	/// <summary>
	/// Генерация строкового сообщения о ошибке
	/// </summary>
	/// <returns>Сообщение об ошибке</returns>
	public override string ToString()
	{
		return "Не найдено: " + Message;
	}
}
