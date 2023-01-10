namespace TomLonghurst.Services.Resilient.Fallback.Models;

internal class ExceptionDelegate
{
    private readonly Func<Exception, Task>? _action1;
    private readonly Func<Exception, IServiceProvider, Task>? _action2;
    private readonly Action<Exception>? _action3;
    private readonly Action<Exception, IServiceProvider>? _action4;

    public ExceptionDelegate(Func<Exception, Task> action)
    {
        _action1 = action;
    }
    
    public ExceptionDelegate(Func<Exception, IServiceProvider, Task> action)
    {
        _action2 = action;
    }
    
    public ExceptionDelegate(Action<Exception> action)
    {
        _action3 = action;
    }
    
    public ExceptionDelegate(Action<Exception, IServiceProvider> action)
    {
        _action4 = action;
    }

    public void Invoke(Exception exception, IServiceProvider serviceProvider)
    {
        _action1?.Invoke(exception);
        _action2?.Invoke(exception, serviceProvider);
        _action3?.Invoke(exception);
        _action4?.Invoke(exception, serviceProvider);
    }
    
    public static implicit operator ExceptionDelegate(Func<Exception, Task> action) => new(action);
    public static implicit operator ExceptionDelegate(Func<Exception, IServiceProvider, Task> action) => new(action);
    public static implicit operator ExceptionDelegate(Action<Exception> action) => new(action);
    public static implicit operator ExceptionDelegate(Action<Exception, IServiceProvider> action) => new(action);
}