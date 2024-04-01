namespace DatalakeDatabase.Exceptions;

public class NotFoundException(string? message) : Exception(message)
{
}
