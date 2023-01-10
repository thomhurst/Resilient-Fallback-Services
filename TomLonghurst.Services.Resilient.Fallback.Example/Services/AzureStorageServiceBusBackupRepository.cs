namespace TomLonghurst.Services.Resilient.Fallback.Example.Services;

public class AzureStorageServiceBusBackupRepository : IServiceBusRepository
{
    public Task Send(string dummyData)
    {
        // Both service buses failed - Try to dump the message data into an azure storage backup
        return Task.CompletedTask;
    }
}