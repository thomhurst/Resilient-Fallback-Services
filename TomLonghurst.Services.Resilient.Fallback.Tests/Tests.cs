using Microsoft.Extensions.DependencyInjection;
using Moq;
using TomLonghurst.Services.Resilient.Fallback.Exceptions;
using TomLonghurst.Services.Resilient.Fallback.Extensions;
using TomLonghurst.Services.Resilient.Fallback.Models;
using TomLonghurst.Services.Resilient.Fallback.Tests.Interfaces;

namespace TomLonghurst.Services.Resilient.Fallback.Tests;

public class Tests
{
    [Test]
    public void When_ServiceSuccessful_Then_DoNotExecuteFallback()
    {
        var myString = "";
        var myException = "";
        
        var mock1 = new Mock<IMyInterface_Child1>();
        mock1.Setup(x => x.DoSomething1())
            .Callback(() => myString += "mock1.DoSomething1");
        
        var mock2 = new Mock<IMyInterface_Child2>();
        mock2.Setup(x => x.DoSomething1())
            .Callback(() => myString += "mock2.DoSomething1");
        
        var mock3 = new Mock<IMyInterface_Child3>();
        mock3.Setup(x => x.DoSomething1())
            .Callback(() => myString += "mock3.DoSomething1");

        var services = new ServiceCollection()
            .AddSingleton<IMyInterface>(mock1.Object)
            .AddSingleton<IMyInterface>(mock2.Object)
            .AddSingleton<IMyInterface>(mock3.Object);

        services
            .AddResilientService<IMyInterface>()
            .FirstUsing(mock1.Object)
            .ThenUsing(mock2.Object)
            .ThenUsing(mock3.Object)
            .OnHandledException(sp => Console.WriteLine());

        var serviceProvider = services.BuildServiceProvider();

        var resilientMyInterface = serviceProvider.GetRequiredService<Resilient<IMyInterface>>();

        resilientMyInterface.Execute(@interface => @interface.DoSomething1());
        
        Assert.That(myString, Is.EqualTo("mock1.DoSomething1"));
        Assert.That(myException, Is.EqualTo(""));
        
        mock1.Verify(x => x.DoSomething1(),
            Times.Once);
        
        mock2.Verify(x => x.DoSomething1(),
            Times.Never);
        
        mock3.Verify(x => x.DoSomething1(),
            Times.Never);
    }
    
    [Test]
    public void Given_ServiceOneThrowsException_And_ServiceTwoSucceeds_Then_ExecuteSecondService_But_NotThirdService()
    {
        var myString = "";
        var myException = "";
        
        var mock1 = new Mock<IMyInterface_Child1>();
        mock1.Setup(x => x.DoSomething1())
            .Throws(new Exception("Blah"));
        
        var mock2 = new Mock<IMyInterface_Child2>();
        mock2.Setup(x => x.DoSomething1())
            .Callback(() => myString += "mock2.DoSomething1");
        
        var mock3 = new Mock<IMyInterface_Child3>();
        mock3.Setup(x => x.DoSomething1())
            .Callback(() => myString += "mock3.DoSomething1");

        var services = new ServiceCollection()
            .AddSingleton<IMyInterface>(mock1.Object)
            .AddSingleton<IMyInterface>(mock2.Object)
            .AddSingleton<IMyInterface>(mock3.Object);

        services
            .AddResilientService<IMyInterface>()
            .FirstUsing(mock1.Object)
            .ThenUsing(mock2.Object)
            .ThenUsing(mock3.Object)
            .OnHandledException(e => myException += e.Message);

        var serviceProvider = services.BuildServiceProvider();

        var resilientMyInterface = serviceProvider.GetRequiredService<Resilient<IMyInterface>>();

        resilientMyInterface.Execute(@interface => @interface.DoSomething1());
        
        Assert.That(myString, Is.EqualTo("mock2.DoSomething1"));
        Assert.That(myException, Is.EqualTo("Blah"));
        
        mock1.Verify(x => x.DoSomething1(),
            Times.Once);
        
        mock2.Verify(x => x.DoSomething1(),
            Times.Once);
        
        mock3.Verify(x => x.DoSomething1(),
            Times.Never);
    }
    
    [Test]
    public void Given_ServiceOneThrowsException_And_ExceptionIsConfiguredToNotFallback_Then_DoNotExecuteSecondService_And_ThrowException()
    {
        var myString = "";
        var myException = "";
        
        var mock1 = new Mock<IMyInterface_Child1>();
        mock1.Setup(x => x.DoSomething1())
            .Throws(new Exception("Blah"));
        
        var mock2 = new Mock<IMyInterface_Child2>();
        mock2.Setup(x => x.DoSomething1())
            .Callback(() => myString += "mock2.DoSomething1");
        
        var mock3 = new Mock<IMyInterface_Child3>();
        mock3.Setup(x => x.DoSomething1())
            .Callback(() => myString += "mock3.DoSomething1");

        var services = new ServiceCollection()
            .AddSingleton<IMyInterface>(mock1.Object)
            .AddSingleton<IMyInterface>(mock2.Object)
            .AddSingleton<IMyInterface>(mock3.Object);

        services
            .AddResilientService<IMyInterface>()
            .FirstUsing(mock1.Object)
            .ThenUsing(mock2.Object)
            .ThenUsing(mock3.Object)
            .OnHandledException(e => myException += e.Message)
            .ConfigureExceptionToNotFallbackOn(@interface => @interface.DoSomething1(), e => e.Message == "Blah");

        var serviceProvider = services.BuildServiceProvider();

        var resilientMyInterface = serviceProvider.GetRequiredService<Resilient<IMyInterface>>();

        Assert.That(() => resilientMyInterface.Execute(@interface => @interface.DoSomething1()),
            Throws.Exception.With.Message.EqualTo("Blah"));
        
        Assert.That(myString, Is.EqualTo(""));
        Assert.That(myException, Is.EqualTo(""));
        
        mock1.Verify(x => x.DoSomething1(),
            Times.Once);
        
        mock2.Verify(x => x.DoSomething1(),
            Times.Never);
        
        mock3.Verify(x => x.DoSomething1(),
            Times.Never);
    }
    
    [Test]
    public void Given_ServiceOne_WithParams_ThrowsException_And_ExceptionIsConfiguredToNotFallback_Then_DoNotExecuteSecondService_And_ThrowException()
    {
        var myString = "";
        var myException = "";
        
        var mock1 = new Mock<IMyInterface_Child1>();
        mock1.Setup(x => x.DoSomethingWithParams1(It.IsAny<int>(), It.IsAny<string>()))
            .Throws(new Exception("Blah"));
        
        var mock2 = new Mock<IMyInterface_Child2>();
        mock2.Setup(x => x.DoSomethingWithParams1(It.IsAny<int>(), It.IsAny<string>()))
            .Callback(() => myString += "mock2.DoSomething1");
        
        var mock3 = new Mock<IMyInterface_Child3>();
        mock3.Setup(x => x.DoSomethingWithParams1(It.IsAny<int>(), It.IsAny<string>()))
            .Callback(() => myString += "mock3.DoSomething1");

        var services = new ServiceCollection()
            .AddSingleton<IMyInterface>(mock1.Object)
            .AddSingleton<IMyInterface>(mock2.Object)
            .AddSingleton<IMyInterface>(mock3.Object);

        services
            .AddResilientService<IMyInterface>()
            .FirstUsing(mock1.Object)
            .ThenUsing(mock2.Object)
            .ThenUsing(mock3.Object)
            .OnHandledException(e => myException += e.Message)
            .ConfigureExceptionToNotFallbackOn(@interface => @interface.DoSomethingWithParams1(Argument.Of<int>(), Argument.Of<string>()), e => e.Message == "Blah");

        var serviceProvider = services.BuildServiceProvider();

        var resilientMyInterface = serviceProvider.GetRequiredService<Resilient<IMyInterface>>();

        Assert.That(() => resilientMyInterface.Execute(@interface => @interface.DoSomethingWithParams1(3, "4")),
            Throws.Exception.With.Message.EqualTo("Blah"));
        
        Assert.That(myString, Is.EqualTo(""));
        Assert.That(myException, Is.EqualTo(""));
        
        mock1.Verify(x => x.DoSomethingWithParams1(It.IsAny<int>(), It.IsAny<string>()),
            Times.Once);
        
        mock2.Verify(x => x.DoSomethingWithParams1(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
        
        mock3.Verify(x => x.DoSomethingWithParams1(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }
    
    [Test]
    public void When_AllServicesFail_Then_Throw_ResilientServicesFailedException()
    {
        var myString = "";
        var myException = "";
        
        var mock1 = new Mock<IMyInterface_Child1>();
        mock1.Setup(x => x.DoSomething1())
            .Throws(new Exception("Blah"));
        
        var mock2 = new Mock<IMyInterface_Child2>();
        mock2.Setup(x => x.DoSomething1())
            .Throws(new Exception("Blah"));
        
        var mock3 = new Mock<IMyInterface_Child3>();
        mock3.Setup(x => x.DoSomething1())
            .Throws(new Exception("Blah"));

        var services = new ServiceCollection()
            .AddSingleton<IMyInterface>(mock1.Object)
            .AddSingleton<IMyInterface>(mock2.Object)
            .AddSingleton<IMyInterface>(mock3.Object);

        services
            .AddResilientService<IMyInterface>()
            .FirstUsing(mock1.Object)
            .ThenUsing(mock2.Object)
            .ThenUsing(mock3.Object)
            .OnHandledException(e => myException += e.Message);

        var serviceProvider = services.BuildServiceProvider();

        var resilientMyInterface = serviceProvider.GetRequiredService<Resilient<IMyInterface>>();

        Assert.That(() => resilientMyInterface.Execute(@interface => @interface.DoSomething1()),
            Throws.Exception.InstanceOf<ResilientServicesFailedException<IMyInterface>>()
                .And.InnerException.Matches<AggregateException>(e => e.InnerExceptions.Count == 3));
        
        Assert.That(myString, Is.EqualTo(""));
        Assert.That(myException, Is.EqualTo("BlahBlahBlah"));
        
        mock1.Verify(x => x.DoSomething1(),
            Times.Once);
        
        mock2.Verify(x => x.DoSomething1(),
            Times.Once);
        
        mock3.Verify(x => x.DoSomething1(),
            Times.Once);
    }
    
    [Test]
    public void Given_PreviouslyFailed_When_NextCallSucceeds_Then_BehavesAsExpected()
    {
        var myString = "";
        var myException = "";
        
        var mock1 = new Mock<IMyInterface_Child1>();
        mock1.Setup(x => x.DoSomethingWithParams1(1, "2"))
            .Throws(new Exception("Blah"));
        
        var mock2 = new Mock<IMyInterface_Child2>();
        mock2.Setup(x => x.DoSomethingWithParams1(1, "2"))
            .Callback(() => myString += "mock2.DoSomething1");
        
        var mock3 = new Mock<IMyInterface_Child3>();
        mock3.Setup(x => x.DoSomethingWithParams1(1, "2"))
            .Callback(() => myString += "mock3.DoSomething1");

        var services = new ServiceCollection()
            .AddSingleton<IMyInterface>(mock1.Object)
            .AddSingleton<IMyInterface>(mock2.Object)
            .AddSingleton<IMyInterface>(mock3.Object);

        services
            .AddResilientService<IMyInterface>()
            .FirstUsing(mock1.Object)
            .ThenUsing(mock2.Object)
            .ThenUsing(mock3.Object)
            .OnHandledException(e => myException += e.Message)
            .ConfigureExceptionToNotFallbackOn(@interface => @interface.DoSomethingWithParams1(Argument.Of<int>(), Argument.Of<string>()), e => e.Message == "Blah");

        var serviceProvider = services.BuildServiceProvider();

        var resilientMyInterface = serviceProvider.GetRequiredService<Resilient<IMyInterface>>();

        Assert.That(() => resilientMyInterface.Execute(@interface => @interface.DoSomethingWithParams1(1, "2")),
            Throws.Exception.With.Message.EqualTo("Blah"));
        
        Assert.That(myString, Is.EqualTo(""));
        Assert.That(myException, Is.EqualTo(""));
        
        mock1.Verify(x => x.DoSomethingWithParams1(It.IsAny<int>(), It.IsAny<string>()),
            Times.Once);
        
        mock2.Verify(x => x.DoSomethingWithParams1(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
        
        mock3.Verify(x => x.DoSomethingWithParams1(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);

        resilientMyInterface.Execute(@interface => @interface.DoSomethingWithParams1(3, "4"));

        mock1.Verify(x => x.DoSomethingWithParams1(It.IsAny<int>(), It.IsAny<string>()),
            Times.Exactly(2));
        
        mock2.Verify(x => x.DoSomethingWithParams1(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
        
        mock3.Verify(x => x.DoSomethingWithParams1(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }
}