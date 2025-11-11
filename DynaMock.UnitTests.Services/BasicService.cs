using System.Security.Cryptography.X509Certificates;

namespace DynaMock.UnitTests.Services;

public interface IBasicService
{
    void DoSomething();
    string GetValue();
    int Add(int first, int second);
    int Count { get; set; }

    event EventHandler? OnBasicEvent;
    event EventHandler<MyEventArgs>? OnEventWithArgs;

    void RaiseBasicEvent();
    void RaiseEventWithArgs(MyEventArgs myEventArgs);
}

public class MyEventArgs(string value) : EventArgs
{
    public string? Value { get; } = value;
}
    
