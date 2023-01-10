namespace TomLonghurst.Services.Resilient.Fallback.Tests.Interfaces;

public interface IMyInterface
{
    void DoSomething1();
    Task DoSomething2();
    
    void DoSomethingWithParams1(int one, string two);
    Task DoSomethingWithParams2(int one, string two);
    
    string DoSomethingWithReturn1();
    Task<string> DoSomethingWithReturn2();
    
    string DoSomethingWithReturnWithParams1(int one, string two);
    Task<string> DoSomethingWithReturnWithParams2(int one, string two);
}