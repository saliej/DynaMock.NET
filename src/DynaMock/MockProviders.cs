using System;

namespace DynaMock;

public interface IMockProvider<T> where T : class
{
    T? Current { get; }
	MockConfiguration<T>? MockConfig { get; }
}

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

public sealed class AsyncLocalMockProvider<T> : IMockProvider<T> where T : class
{
	private AsyncLocal<T?> _mock = new();
	private AsyncLocal<MockConfiguration<T>?> _mockConfig = new();

	public T? Current => _mock.Value;

	public MockConfiguration<T>? MockConfig => _mockConfig.Value;

	public void SetMock(T? mock, Action<MockConfiguration<T>>? configureOptions = null)
	{
		_mock.Value = mock;

		if (configureOptions != null && mock != null)
		{
			var config = new MockConfiguration<T>();
			configureOptions(config);
			_mockConfig.Value = config;
		}
		else
		{
			_mockConfig.Value = null;
		}
	}

	public void RemoveMock() => SetMock(null);
}