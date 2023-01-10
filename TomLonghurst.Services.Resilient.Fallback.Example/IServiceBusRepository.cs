namespace TomLonghurst.Services.Resilient.Fallback.Example;

public interface IServiceBusRepository
{
    public Task Send(string dummyData);
}