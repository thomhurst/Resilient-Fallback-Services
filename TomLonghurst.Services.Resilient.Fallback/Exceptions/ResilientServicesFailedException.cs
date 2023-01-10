namespace TomLonghurst.Services.Resilient.Fallback.Exceptions;

public class ResilientServicesFailedException : ResilientServicesException
{
    protected ResilientServicesFailedException(string message, Exception? baseException) : base(message, baseException)
    {
    }
}
public class ResilientServicesFailedException<T> : ResilientServicesFailedException
{
    internal ResilientServicesFailedException(IReadOnlyCollection<Exception>? caughtExceptions) : base($"All Registered Fallbacks for {typeof(T).FullName} were unsuccessful", GetBaseException(caughtExceptions))
    {
    }

    private static Exception? GetBaseException(IReadOnlyCollection<Exception>? caughtExceptions)
    {
        if (caughtExceptions == null || caughtExceptions.Count == 0)
        {
            return null;
        }

        if (caughtExceptions.Count == 1)
        {
            return caughtExceptions.First();
        }

        return new AggregateException(caughtExceptions);
    }
}