namespace DatalakeDatabase.Exceptions;

public class DatabaseException(string? message) : Exception(message)
{
	public override string ToString()
	{
		return "Ошибка базы данных: " + Message;
	}
}
