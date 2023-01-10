namespace TomLonghurst.Services.Resilient.Fallback.Exceptions;

public class ResilientServicesInitializationException : ResilientServicesException
{
    public ResilientServicesInitializationException(string message) : base(message)
    {
    }
}