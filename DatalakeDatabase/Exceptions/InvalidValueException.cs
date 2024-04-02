namespace DatalakeDatabase.Exceptions;

public class InvalidValueException(string? message) : Exception(message)
{
	public override string ToString()
	{
		return "Неверное значение: " + Message;
	}
}
