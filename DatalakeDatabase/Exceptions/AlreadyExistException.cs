namespace DatalakeDatabase.Exceptions;

public class AlreadyExistException(string? message) : Exception(message)
{
}
