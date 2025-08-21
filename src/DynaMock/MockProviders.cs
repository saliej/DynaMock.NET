using System;
using System.Security.Cryptography;

namespace DynaMock;

public interface IMockProvider<T> where T : class
{
    T? Current { get; }
	MockConfiguration<T>? MockConfig { get; }
}

//public sealed class AsyncLocalMockProvider<T> : IMockProvider<T> where T : class
//{
//    private static readonly AsyncLocal<T?> _current = new();
    
//    public T? Current => _current.Value;

//	public MockConfiguration<T>? MockConfig => _mockConfig;

//	public static void Set(T? instance) => _current.Value = instance;
//    public static void Remove() => _current.Value = null;
//}

public sealed class DefaultMockProvider<T> : IMockProvider<T> where T : class
{
    private static T? _mock;
	private static MockConfiguration<T>? _mockConfig;

	public T? Current => _mock;

	public MockConfiguration<T>? MockConfig => _mockConfig;

	public static void SetMock(T? mock, Action<MockConfiguration<T>>? configureOptions = null)
    {
        _mock = mock;

        if (configureOptions != null && mock != null)
        {
	        var config = new MockConfiguration<T>();
            configureOptions(config);
            _mockConfig = config;
        }
        else
        {
	        _mockConfig = null;
        }
    }

	public static void RemoveMock() => SetMock(null);
}
