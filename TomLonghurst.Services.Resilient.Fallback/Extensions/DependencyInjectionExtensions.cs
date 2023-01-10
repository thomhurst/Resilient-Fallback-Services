using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TomLonghurst.Services.Resilient.Fallback.Builders;
using TomLonghurst.Services.Resilient.Fallback.Exceptions;

namespace TomLonghurst.Services.Resilient.Fallback.Extensions;

public static class DependencyInjectionExtensions
{
    public static IncompleteResilientServiceBuilder<T> AddResilientService<T>(this IServiceCollection services)
    {
        services.TryAddSingleton(typeof(Resilient<>), typeof(ResilientImpl<>));
        
        var resilientServiceBuilder = new IncompleteResilientServiceBuilder<T>(services);
        
        var serviceDescriptor = new ServiceDescriptor(typeof(IIncompleteResilientServiceBuilder), f => resilientServiceBuilder,
            ServiceLifetime.Singleton);

        resilientServiceBuilder.AddServiceDescriptor(serviceDescriptor);

        services.AddSingleton(resilientServiceBuilder);
        services.Add(serviceDescriptor);
        
        return resilientServiceBuilder;
    }

    public static IServiceProvider ValidateResilientServices(this IServiceProvider serviceProvider)
    {
        var incompleteRegistrations = serviceProvider.GetServices<IIncompleteResilientServiceBuilder>().ToArray();
        
        if (incompleteRegistrations.Any())
        {
            var names = incompleteRegistrations.Select(x => x.Type.FullName);
            throw new ResilientServicesInitializationException($"Resilient Services have been registered without order of services configured: {string.Join(Environment.NewLine, names)}");
        }

        var registrationsWithOneImplementation = serviceProvider.GetServices<IThenResilientServiceBuilder>()
            .Where(rsb => rsb.ImplementationCount <= 1)
            .ToArray();
        
        if (registrationsWithOneImplementation.Any())
        {
            var names = registrationsWithOneImplementation.Select(x => x.Type.FullName);
            throw new ResilientServicesInitializationException($"Resilient Services have been registered without fallbacks: {string.Join(Environment.NewLine, names)}");
        }

        return serviceProvider;
    }
}