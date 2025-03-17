using Datalake.PublicApi.Exceptions.Base;

namespace Datalake.PublicApi.Exceptions;

/// <summary>
/// Ошибка при попытке добавления чего-то, что уже существует
/// </summary>
/// <param name="message">Сообщение после "Уже существует: "</param>
public class AlreadyExistException(string? message) : DatalakeException(message)
{
	/// <summary>
	/// Генерация строкового сообщения о ошибке
	/// </summary>
	/// <returns>Сообщение об ошибке</returns>
	public override string ToString()
	{
		return "Уже существует: " + Message;
	}
}
