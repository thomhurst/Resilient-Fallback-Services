using System.Linq.Expressions;

namespace TomLonghurst.Services.Resilient.Fallback;

public interface Resilient<T> where T : class
{
    void Execute(Expression<Action<T>> action);
    TResult Execute<TResult>(Expression<Func<T, TResult>> action);
    
    Task<TResult> ExecuteAsync<TResult>(Expression<Func<T, Task<TResult>>> action);
    Task ExecuteAsync(Expression<Func<T, Task>> action);
}