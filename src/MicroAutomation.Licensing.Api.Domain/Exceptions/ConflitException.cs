using System;

namespace MicroAutomation.Licensing.Api.Domain.Exceptions;

public class ConflitException : Exception
{
    public ConflitException()
    { }

    public ConflitException(string message)
        : base(message) { }

    public ConflitException(string message, Exception innerException)
        : base(message, innerException) { }
}