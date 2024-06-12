using Datalake.ApiClasses.Exceptions.Base;

namespace Datalake.ApiClasses.Exceptions;

/// <summary>
/// Ошибка при попытке выполнить действие, выполнение которого защищено по соображениям безопасности
/// </summary>
/// <param name="message">Сообщение после "Запрещено: "</param>
public class ForbiddenException(string? message) : DatalakeException(message)
{
	/// <summary>
	/// Генерация строкового сообщения о ошибке
	/// </summary>
	/// <returns>Сообщение об ошибке</returns>
	public override string ToString()
	{
		return "Запрещено: " + Message;
	}
}
