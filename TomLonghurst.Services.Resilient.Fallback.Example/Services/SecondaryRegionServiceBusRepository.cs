namespace TomLonghurst.Services.Resilient.Fallback.Example.Services;

public class SecondaryRegionServiceBusRepository : IServiceBusRepository
{
    public Task Send(string dummyData)
    {
        // Send to Secondary Region Service Bus, otherwise throw exception
        return Task.CompletedTask;
    }
}