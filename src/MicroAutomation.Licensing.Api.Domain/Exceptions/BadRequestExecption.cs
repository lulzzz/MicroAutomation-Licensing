using System;

namespace MicroAutomation.Licensing.Api.Domain.Exceptions;

public class BadRequestExecption : Exception
{
    public BadRequestExecption()
    { }

    public BadRequestExecption(string message)
        : base(message) { }

    public BadRequestExecption(string message, Exception innerException)
        : base(message, innerException) { }
}