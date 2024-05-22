namespace DatalakeDatabase.Exceptions.Base;

public abstract class DatalakeException(string? message) : Exception(message)
{
}
