using System;
using DynaMock.UnitTests.Support;

namespace DynaMock.UnitTests.TestDoubles;

public class MockableAbstractTestServiceDouble : MockableBase<AbstractTestService>
{
    public MockableAbstractTestServiceDouble(AbstractTestService realImplementation) : base(realImplementation)
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

    public string GetDefaultValue()
    {
        if (ShouldUseMockForMethod("GetDefaultValue"))
            return Mock.GetDefaultValue();
        return _realImplementation.GetDefaultValue();
    }
}