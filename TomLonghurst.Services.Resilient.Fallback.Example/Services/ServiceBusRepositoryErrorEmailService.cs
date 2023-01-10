namespace TomLonghurst.Services.Resilient.Fallback.Example.Services;

public class ServiceBusRepositoryErrorEmailService : IServiceBusRepository
{
    public Task Send(string dummyData)
    {
        // Send a critical email that all attempted servicebus services are failing
        return Task.CompletedTask;
    }
}