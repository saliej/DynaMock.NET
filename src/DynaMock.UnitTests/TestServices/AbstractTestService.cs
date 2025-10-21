using System;

namespace DynaMock.UnitTests.TestServices;

public abstract class AbstractTestService
{
    public abstract string GetValue();
    public abstract Task<int> GetCountAsync();
    
    public virtual string GetDefaultValue() => "Default";
    
    protected abstract void OnValueChanged();
    
    public string Name { get; set; } = string.Empty;
}