// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TomLonghurst.Services.Resilient.Fallback;
using TomLonghurst.Services.Resilient.Fallback.Example;
using TomLonghurst.Services.Resilient.Fallback.Example.Services;
using TomLonghurst.Services.Resilient.Fallback.Extensions;
using TomLonghurst.Services.Resilient.Fallback.Models;

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
    