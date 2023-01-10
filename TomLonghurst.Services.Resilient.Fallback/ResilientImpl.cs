using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.Services.Resilient.Fallback.Builders;
using TomLonghurst.Services.Resilient.Fallback.Exceptions;

namespace TomLonghurst.Services.Resilient.Fallback;

internal class ResilientImpl<T> : Resilient<T> where T : class
{
    private readonly ThenResilientServiceBuilder<T> _thenResilientServiceBuilder;
    private readonly IServiceProvider _serviceProvider;

    public ResilientImpl(ThenResilientServiceBuilder<T> thenResilientServiceBuilder, IServiceProvider serviceProvider)
    {
        _thenResilientServiceBuilder = thenResilientServiceBuilder;
        _serviceProvider = serviceProvider;
    }

    public void Execute(Expression<Action<T>> action)
    {
        var methodInfo = (action.Body as MethodCallExpression)?.Method;
        
        if (methodInfo == null)
        {
            throw new ArgumentNullException(nameof(methodInfo));
        }

        var implementations = GetImplementations();
        
        List<Exception>? caughtExceptions = null;

        foreach (var implementation in implementations)
        {
            try
            {
                action.Compile()(implementation);
                return;
            }
            catch (Exception e)
            {
                if (AnyExceptionsToThrow(methodInfo, e))
                {
                    throw;
                }

                InvokeCaughtExceptionHandler(e);
                
                caughtExceptions ??= new List<Exception>();
                caughtExceptions.Add(e);
            }
        }
        
        throw new ResilientServicesFailedException<T>(caughtExceptions);
    }

    public TResult Execute<TResult>(Expression<Func<T, TResult>> action)
    {
        var methodInfo = (action.Body as MethodCallExpression)?.Method;
        
        if (methodInfo == null)
        {
            throw new ArgumentNullException(nameof(methodInfo));
        }

        var implementations = GetImplementations();
        
        List<Exception>? caughtExceptions = null;

        foreach (var implementation in implementations)
        {
            try
            {
                return action.Compile()(implementation);
            }
            catch (Exception e)
            {
                if (AnyExceptionsToThrow(methodInfo, e))
                {
                    throw;
                }

                InvokeCaughtExceptionHandler(e);
                
                caughtExceptions ??= new List<Exception>();
                caughtExceptions.Add(e);
            }
        }
        
        throw new ResilientServicesFailedException<T>(caughtExceptions);
    }

    public async Task<TResult> ExecuteAsync<TResult>(Expression<Func<T, Task<TResult>>> action)
    {
        var methodInfo = (action.Body as MethodCallExpression)?.Method;
        
        if (methodInfo == null)
        {
            throw new ArgumentNullException(nameof(methodInfo));
        }
        
        var implementations = GetImplementations();
        
        List<Exception>? caughtExceptions = null;
        
        foreach (var implementation in implementations)
        {
            try
            {
                return await action.Compile()!(implementation);
            }
            catch (Exception e)
            {
                if (AnyExceptionsToThrow(methodInfo, e))
                {
                    throw;
                }

                InvokeCaughtExceptionHandler(e);
                
                caughtExceptions ??= new List<Exception>();
                caughtExceptions.Add(e);
            }
        }

        throw new ResilientServicesFailedException<T>(caughtExceptions);
    }

    public async Task ExecuteAsync(Expression<Func<T, Task>> action)
    {
        var methodInfo = (action.Body as MethodCallExpression)?.Method;
        
        if (methodInfo == null)
        {
            throw new ArgumentNullException(nameof(methodInfo));
        }
        
        var implementations = GetImplementations();

        List<Exception>? caughtExceptions = null;
        
        foreach (var implementation in implementations)
        {
            try
            {
                await action.Compile()(implementation);
                return;
            }
            catch (Exception e)
            {
                if (AnyExceptionsToThrow(methodInfo, e))
                {
                    throw;
                }

                InvokeCaughtExceptionHandler(e);

                caughtExceptions ??= new List<Exception>();
                caughtExceptions.Add(e);
            }
        }
        
        throw new ResilientServicesFailedException<T>(caughtExceptions);
    }

    private IEnumerable<T?> GetImplementations()
    {
        var registeredUnderInterface = _serviceProvider.GetServices(typeof(T));
        
        foreach (var implementation in _thenResilientServiceBuilder.Implementations)
        {
            if (implementation.Instance != null)
            {
                yield return implementation.Instance as T;
                continue;
            }

            if (implementation.Factory != null)
            {
                yield return implementation.Factory(_serviceProvider) as T;
                continue;
            }
            
            yield return registeredUnderInterface.FirstOrDefault(x => x.GetType() == implementation.Type) as T
                         ?? ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider,
                             implementation.Type) as T;
        }
    }

    private void InvokeCaughtExceptionHandler(Exception exception)
    {
        _thenResilientServiceBuilder.ExceptionHandler?.Invoke(exception, _serviceProvider);
    }

    private bool AnyExceptionsToThrow(MethodInfo methodInfo, Exception e)
    {
        return _thenResilientServiceBuilder.ExceptionsToNotFallbackOnDictionary.TryGetValue(methodInfo, out var exceptionsToNotFallbackOn)
               && exceptionsToNotFallbackOn.Any(func => func(e));
    }
}