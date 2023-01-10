using Microsoft.Extensions.DependencyInjection;
using Moq;
using TomLonghurst.Services.Resilient.Fallback.Exceptions;
using TomLonghurst.Services.Resilient.Fallback.Extensions;
using TomLonghurst.Services.Resilient.Fallback.Tests.Interfaces;

namespace TomLonghurst.Services.Resilient.Fallback.Tests;

public class ValidateTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Given_NoServicesAdded_When_ValidatedCalled_Then_Throw_ResilientServicesInitializationException()
    {
        var mock1 = new Mock<IMyInterface_Child1>();
        var mock2 = new Mock<IMyInterface_Child2>();
        var mock3 = new Mock<IMyInterface_Child3>();

        var services = new ServiceCollection()
            .AddSingleton<IMyInterface>(mock1.Object)
            .AddSingleton<IMyInterface>(mock2.Object)
            .AddSingleton<IMyInterface>(mock3.Object);

        services
            .AddResilientService<IMyInterface>();

        var serviceProvider = services.BuildServiceProvider();

        Assert.That(() => serviceProvider.ValidateResilientServices(),
            Throws.InstanceOf<ResilientServicesInitializationException>()
            .And.Message.EqualTo("Resilient Services have been registered without order of services configured: TomLonghurst.Services.Resilient.Fallback.Tests.Interfaces.IMyInterface"));
    }

    [Test]
    public void Given_NoFallbacksAdded_When_ValidatedCalled_Then_Throw_ResilientServicesInitializationException()
    {
        var mock1 = new Mock<IMyInterface_Child1>();
        var mock2 = new Mock<IMyInterface_Child2>();
        var mock3 = new Mock<IMyInterface_Child3>();

        var services = new ServiceCollection()
            .AddSingleton<IMyInterface>(mock1.Object)
            .AddSingleton<IMyInterface>(mock2.Object)
            .AddSingleton<IMyInterface>(mock3.Object);

        services
            .AddResilientService<IMyInterface>()
            .FirstUsing(mock1.Object);

        var serviceProvider = services.BuildServiceProvider();

        Assert.That(() => serviceProvider.ValidateResilientServices(),
            Throws.InstanceOf<ResilientServicesInitializationException>()
                .And.Message.EqualTo("Resilient Services have been registered without fallbacks: TomLonghurst.Services.Resilient.Fallback.Tests.Interfaces.IMyInterface"));
    }
    
    [Test]
    public void Given_PrimaryServicesWithFallbacksConfigured_When_ValidatedCalled_Then_Succeed()
    {
        var mock1 = new Mock<IMyInterface_Child1>();
        var mock2 = new Mock<IMyInterface_Child2>();
        var mock3 = new Mock<IMyInterface_Child3>();

        var services = new ServiceCollection()
            .AddSingleton<IMyInterface>(mock1.Object)
            .AddSingleton<IMyInterface>(mock2.Object)
            .AddSingleton<IMyInterface>(mock3.Object);

        services
            .AddResilientService<IMyInterface>()
            .FirstUsing(mock1.Object)
            .ThenUsing(mock2.Object);

        var serviceProvider = services.BuildServiceProvider();

        Assert.That(() => serviceProvider.ValidateResilientServices(), Throws.Nothing);
    }
}