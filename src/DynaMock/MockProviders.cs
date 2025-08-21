using System;

namespace DynaMock;

public interface IMockProvider<out T> { T? Current { get; } }

public sealed class AsyncLocalMockProvider<T> : IMockProvider<T> where T : class
{
    private static readonly AsyncLocal<T?> _current = new();
    
    public T? Current => _current.Value;
    
    public static void Set(T? instance) => _current.Value = instance;
    public static void Remove() => _current.Value = null;
}

public sealed class ScopedMockProvider<T> : IMockProvider<T>, IDisposable where T : class
{
    private T? _mock;
    
    public T? Current => _mock;
    
    public void SetMock(T? mock) => _mock = mock;
    public void Dispose() => _mock = null;
}

public sealed class AsyncLocalScopedMockProvider<T> : IMockProvider<T> where T : class
{
    private static readonly AsyncLocal<T?> _current = new();
    
    public T? Current => _current.Value;
    
    public static void SetMock(T? instance) => _current.Value = instance;
    public static void RemoveMock() => _current.Value = null;
    
    public static IDisposable SetScopedMock(T? instance)
    {
        SetMock(instance);
        return new MockScope(RemoveMock);
    }
}