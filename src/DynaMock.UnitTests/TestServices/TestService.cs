using System;

namespace DynaMock.UnitTests.TestServices;

public interface ITestService
{
    string GetValue();
    Task<int> GetCountAsync();
    void SetValue(string value);
    string Name { get; set; }
    event EventHandler ValueChanged;
}

public class TestService : ITestService
{
    public string Name { get; set; } = string.Empty;
    public event EventHandler? ValueChanged;

    public string GetValue() => "Test";
    public Task<int> GetCountAsync() => Task.FromResult(0);
    public void SetValue(string value) { }
}