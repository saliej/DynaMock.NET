using System;
using DynaMock.UnitTests.Support;

namespace DynaMock.UnitTests.TestDoubles;

public class MockableITestServiceDouble : MockableBase<ITestService>, ITestService
{
    public MockableITestServiceDouble(ITestService realImplementation) : base(realImplementation)
    {
    }

    public string Name
    {
        get
        {
            if (ShouldUseMockForProperty("Name"))
                return Mock.Name;
            return _realImplementation.Name;
        }
        set
        {
            if (ShouldUseMockForProperty("Name"))
                Mock.Name = value;
            else
                _realImplementation.Name = value;
        }
    }

    public event EventHandler? ValueChanged
    {
        add => Implementation.ValueChanged += value;
        remove => Implementation.ValueChanged -= value;
    }

    public string GetValue()
    {
        if (ShouldUseMockForMethod("GetValue"))
            return Mock.GetValue();
        return _realImplementation.GetValue();
    }

    public Task<int> GetCountAsync()
    {
        if (ShouldUseMockForMethod("GetCountAsync"))
            return Mock.GetCountAsync();
        return _realImplementation.GetCountAsync();
    }

    public void SetValue(string value)
    {
        if (ShouldUseMockForMethod("SetValue"))
        {
            Mock.SetValue(value);
            return;
        }
        _realImplementation.SetValue(value);
    }
}