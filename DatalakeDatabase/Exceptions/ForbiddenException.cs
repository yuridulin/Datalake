﻿namespace DatalakeDatabase.Exceptions;

public class ForbiddenException(string? message) : Exception(message)
{
}
