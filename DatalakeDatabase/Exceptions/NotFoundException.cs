namespace DatalakeDatabase.Exceptions;

public class NotFoundException(string? message) : Exception(message)
{
	public override string ToString()
	{
		return "Не найдено: " + Message;
	}
}
