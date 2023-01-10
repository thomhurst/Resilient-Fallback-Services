using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.Services.Resilient.Fallback.Models;

namespace TomLonghurst.Services.Resilient.Fallback.Builders;

public class IncompleteResilientServiceBuilder<T> : IIncompleteResilientServiceBuilder
{
    private readonly IServiceCollection _services;
    private ServiceDescriptor _serviceDescriptor = null!;

    internal IncompleteResilientServiceBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public ThenResilientServiceBuilder<T> FirstUsing<T1>() where T1 : T
    {
        return FirstUsing(new Implementation<T1>());
    }
    
    public ThenResilientServiceBuilder<T> FirstUsing(T tObject)
    {
        if (tObject == null)
        {
            throw new ArgumentNullException(nameof(tObject));
        }
        
        return FirstUsing(new Implementation<T>(tObject));
    }
    
    public ThenResilientServiceBuilder<T> FirstUsing(Func<IServiceProvider, object> factory)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        return FirstUsing(new Implementation<T>(factory));
    }

    private ThenResilientServiceBuilder<T> FirstUsing(Implementation implementation)
    {
        var thenResilientServiceBuilder = new ThenResilientServiceBuilder<T>(implementation, _services);

        _services.Remove(_serviceDescriptor);

        _services.AddSingleton(thenResilientServiceBuilder);

        return thenResilientServiceBuilder;
    }

    public Type Type { get; } = typeof(T);

    public void AddServiceDescriptor(ServiceDescriptor serviceDescriptor)
    {
        _serviceDescriptor = serviceDescriptor;
    }
}