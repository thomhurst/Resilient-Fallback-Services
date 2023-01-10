# Resilient Fallback Services

## Install

Install using NuGet:
`Install-Package TomLonghurst.Services.Resilient.Fallback`

## Overview

This package enables you to code resilient services that will automatically fallback to a 2nd/3rd/4th/5th/etc service if any errors were encountered.

Things are great when they work, but sometimes they don't. And even if that's just 0.001% of the time, sometimes that data we're handling is incredibly important and we need fallbacks in scenarios where our services are down.

## Usage

1.  Define an interface for whatever you need to do
2.  Create multiple implementations of your interface - This would generally be your primary implementation, then a secondary service to fallback to, and then optionally further fallbacks.
3.  Register these implementations in your ServiceCollection in startup, under that interface, as you generally would
4.  On your ServiceCollection, call `AddResilientService<[MyInterfaceType]>` - Passing in the type of your interface
5.  Chain calls to this, setting up your primary implementation and then the order of your fallbacks
6.  Optionally set any configuration, such as a handler for a handled exception (when an exception occurs, but we handle it and then execute the fallback service)
7.  Wherever you want to use this service, inject in `Resilient<[MyInterfaceType]>`

This might look like this:

```csharp
var services = new ServiceCollection()
    .AddSingleton<IServiceBusRepository, AzureStorageServiceBusBackupRepository>()
    .AddSingleton<IServiceBusRepository, SecondaryRegionServiceBusRepository>()
    .AddSingleton<IServiceBusRepository, PrimaryRegionServiceBusRepository>()
    .AddSingleton<IServiceBusRepository, ServiceBusRepositoryErrorEmailService>()
    .AddLogging(builder => builder.AddSimpleConsole());

services.AddResilientService<IServiceBusRepository>()
    .FirstUsing<PrimaryRegionServiceBusRepository>()
    .ThenUsing<SecondaryRegionServiceBusRepository>()
    .ThenUsing<AzureStorageServiceBusBackupRepository>()
    .ThenUsing<ServiceBusRepositoryErrorEmailService>()
    .OnHandledException((ex, sp) => sp.GetRequiredService<ILogger>().LogError(ex, "ResilientService Failure"))
    .ConfigureExceptionToNotFallbackOn(repository =>
            repository.Send(Argument.Of<string>()),
        exception => exception is InvalidDataException
    );
```

```csharp
public class MyService
{
    private readonly Resilient<IServiceBusRepository> _myResilientInterface;

    public MyService(Resilient<IServiceBusRepository> myResilientInterface)
    {
        _myResilientInterface = myResilientInterface;
    }

    public void DoSomething()
    {
        // ...

        _myResilientInterface.ExecuteAsync(myInterface => myInterface.Send("Some data!"));

        // ...
    }
}
```

## Flow

Using the above as an example, here's what's happening.

-   We are setting up a `Resilient<IServiceBusRepository>`, by calling `services.AddResilientService<IServiceBusRepository>()`
-   We are saying that the primary implementation to use is `PrimaryRegionServiceBusRepository`
-   Then if that throws an exception, use `SecondaryRegionServiceBusRepository`
-   Then if that throws an exception, use `AzureStorageServiceBusBackupRepository`
-   Then if that throws an exception, use `ServiceBusRepositoryErrorEmailService`
-   If we hit an exception and successfully fallback to the next service, the defined delegate will be executed, which here logs the error with the ILogger
-   However, if the method is `Send` with the `String` argument, and the exception is `InvalidDataException`, we will not handle the exception and invoke the next service, and so the error will be thrown back to the user and no fallbacks executed

## FAQ

Q: How do you configure arguments to your method in the `ConfigureExceptionToNotFallbackOn` method?
A: Argument values will be ignored, only types are honoured - You should instead use `Argument.Of<T>` for readability

Q: What happens if all my services throw an exception and there are no more fallbacks?
A: A `ResilientServicesFailedException` will be thrown back to the caller, containing inner exceptions with the failures
