using System;

namespace DynaMock;

public class MockScope(Action cleanup) : IDisposable
{
    public void Dispose() => cleanup();
}
