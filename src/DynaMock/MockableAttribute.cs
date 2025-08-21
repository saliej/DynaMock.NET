using System;

namespace DynaMock;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
public class MockableAttribute : Attribute
{
    public List<Type> MockTypes { get; set; }
    public MockableAttribute(params Type [] mockTypes)
    {
        MockTypes = [..mockTypes];
    }
}