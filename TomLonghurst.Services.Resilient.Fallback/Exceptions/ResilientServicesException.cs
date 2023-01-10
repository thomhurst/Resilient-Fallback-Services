using System.Runtime.Serialization;

namespace TomLonghurst.Services.Resilient.Fallback.Exceptions;

public abstract class ResilientServicesException : Exception
{
    protected ResilientServicesException()
    {
    }

    protected ResilientServicesException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected ResilientServicesException(string? message) : base(message)
    {
    }

    protected ResilientServicesException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}