namespace DatalakeDatabase.Exceptions;

public class ForbiddenException(string? message) : Exception(message)
{
	public override string ToString()
	{
		return "Запрещено: " + Message;
	}
}
