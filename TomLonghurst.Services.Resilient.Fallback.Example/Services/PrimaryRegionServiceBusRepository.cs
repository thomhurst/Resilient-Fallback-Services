namespace TomLonghurst.Services.Resilient.Fallback.Example.Services;

public class PrimaryRegionServiceBusRepository : IServiceBusRepository
{
    public Task Send(string dummyData)
    {
        // Send to Primary Region Service Bus, otherwise throw exception
        return Task.CompletedTask;
    }
}