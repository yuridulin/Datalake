namespace DatalakeDatabase.Exceptions;

public class InvalidValueException(string? message) : Exception(message)
{
}
