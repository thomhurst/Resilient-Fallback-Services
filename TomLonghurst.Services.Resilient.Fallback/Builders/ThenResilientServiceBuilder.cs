using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.Services.Resilient.Fallback.Models;

namespace TomLonghurst.Services.Resilient.Fallback.Builders;

public class ThenResilientServiceBuilder<T> : IThenResilientServiceBuilder
{
    private readonly IServiceCollection _services;
    internal readonly List<Implementation> Implementations = new();
    internal readonly ConcurrentDictionary<MethodInfo, List<Func<Exception, bool>>> ExceptionsToNotFallbackOnDictionary = new();
    internal ExceptionDelegate? ExceptionHandler;
    private readonly ServiceDescriptor _serviceDescriptor;

    internal ThenResilientServiceBuilder(Implementation implementation, IServiceCollection services)
    {
        Implementations.Add(implementation);
        _services = services;
        
        _serviceDescriptor =
            new ServiceDescriptor(typeof(IThenResilientServiceBuilder), this);
        _services.Add(_serviceDescriptor);
    }

    public ThenResilientServiceBuilder<T> ThenUsing<T1>() where T1 : T
    {
        return ThenUsing(new Implementation<T1>());
    }

    public ThenResilientServiceBuilder<T> ThenUsing(T tObject)
    {
        if (tObject == null)
        {
            throw new ArgumentNullException(nameof(tObject));
        }

        return ThenUsing(new Implementation<T>(tObject));
    }
    
    public ThenResilientServiceBuilder<T> ThenUsing(Func<IServiceProvider, object> factory)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        return ThenUsing(new Implementation<T>(factory));
    }

    private ThenResilientServiceBuilder<T> ThenUsing(Implementation implementation)
    {
        Implementations.Add(implementation);

        _services.Remove(_serviceDescriptor);

        return this;
    }

    public ThenResilientServiceBuilder<T> ConfigureExceptionToNotFallbackOn(Expression<Func<T, Task>> method, Func<Exception, bool> exceptionToNotFallbackOn)
    {
        return AddException((method.Body as MethodCallExpression)?.Method, exceptionToNotFallbackOn);
    }

    public ThenResilientServiceBuilder<T> ConfigureExceptionToNotFallbackOn(Expression<Action<T>> method, Func<Exception, bool> exceptionToNotFallbackOn)
    {
        return AddException((method.Body as MethodCallExpression)?.Method, exceptionToNotFallbackOn);
    }

    public ThenResilientServiceBuilder<T> OnHandledException(Func<Exception, Task> action)
    {
        ExceptionHandler = action;
        return this;
    }

    public ThenResilientServiceBuilder<T> OnHandledException(Action<Exception> action)
    {
        ExceptionHandler = action;
        return this;
    }

    public ThenResilientServiceBuilder<T> OnHandledException(Func<Exception, IServiceProvider, Task> action)
    {
        ExceptionHandler = action;
        return this;
    }

    public ThenResilientServiceBuilder<T> OnHandledException(Action<Exception, IServiceProvider> action)
    {
        ExceptionHandler = action;
        return this;
    }

    private ThenResilientServiceBuilder<T> AddException(MethodInfo? methodInfo, Func<Exception, bool> exceptionToNotFallbackOn)
    {
        if (methodInfo == null)
        {
            throw new ArgumentNullException(nameof(methodInfo));
        }

        if (exceptionToNotFallbackOn == null)
        {
            throw new ArgumentNullException(nameof(exceptionToNotFallbackOn));
        }

        var exceptionsToNotFallbackOn = ExceptionsToNotFallbackOnDictionary.GetOrAdd(methodInfo, info => new List<Func<Exception, bool>>());

        exceptionsToNotFallbackOn.Add(exceptionToNotFallbackOn);

        return this;
    }

    public int ImplementationCount => Implementations.Count;
    public Type Type { get; } = typeof(T);
}

