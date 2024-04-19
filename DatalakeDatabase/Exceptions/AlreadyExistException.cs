namespace DatalakeDatabase.Exceptions;

public class AlreadyExistException(string? message) : Exception(message)
{
	public override string ToString()
	{
		return "Уже существует: " + Message;
	}
}
