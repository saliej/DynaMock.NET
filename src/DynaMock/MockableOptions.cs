using System;
using System.Collections.Concurrent;

namespace DynaMock;

public class MockableOptions
{
    public bool ThrowOnMissingMock { get; set; }
    public IDictionary<Type, Type> GenericTypeMappings { get; }
        = new ConcurrentDictionary<Type, Type>();
}