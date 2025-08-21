using System;
using System.Reflection;

namespace DynaMock;

//public abstract class MockableBase<T> where T : class
//{
//    protected T _realImplementation;
//	private readonly IMockProvider<T>? _mockProvider;
//	private static T? _mock;
//    private static MockConfiguration<T>? _mockConfig = new();

//    protected MockableBase(T realImplementation, IMockProvider<T>? mockProvider = null)
//    {
//        _realImplementation = realImplementation;
//		_mockProvider = mockProvider = new ScopedMockProvider<T>();
//	}

//    protected static T? Mock => _mock;
//    protected static MockConfiguration<T>? MockConfig => _mockConfig;

//    public static void SetMock(T? mockedService) =>
//        SetMock(mockedService, null);

//    public static void SetMock(T? mockedService, Action<MockConfiguration<T>>? configureOptions)
//    {
//        _mock = mockedService;

//        if (configureOptions != null && mockedService != null)
//        {
//            var config = new MockConfiguration<T>();
//            configureOptions(config);
//            _mockConfig = config;
//        }
//        else
//        {
//            _mockConfig = null;
//        }
//    }

//    public static void RemoveMock()
//    {
//        _mock = null;
//        _mockConfig = null;
//    }

//    public static IDisposable SetScopedMock(T? mockedService, Action<MockConfiguration<T>>? configureOptions = null)
//    {
//        var previousMock = _mock;
//        var previousConfig = _mockConfig;

//        SetMock(mockedService, configureOptions);

//        return new MockScope(() =>
//        {
//            _mock = previousMock;
//            _mockConfig = previousConfig;
//        });
//    }

//    protected T Implementation => Mock ?? _realImplementation;

//    // Helper methods for generated code
//    protected bool ShouldUseMockForMethod(string methodName)
//    {
//        return Mock != null && (MockConfig?.IsMethodMocked(methodName) ?? true);
//    }

//    protected bool ShouldUseMockForProperty(string propertyName)
//    {
//        return Mock != null && (MockConfig?.IsPropertyMocked(propertyName) ?? true);
//    }
//}

public abstract class MockableBase<T> where T : class
{
	protected readonly T RealImplementation;
	protected readonly IMockProvider<T> MockProvider;

	protected MockableBase(T realImplementation, IMockProvider<T>? mockProvider)
	{
		RealImplementation = realImplementation;
		MockProvider = mockProvider ?? new DefaultMockProvider<T>();
	}

	protected T Implementation => MockProvider.Current ?? RealImplementation;

	protected bool ShouldUseMockForMethod(string methodName)
	{
		return MockProvider.Current != null && 
		(
			MockProvider.MockConfig == null ||
			MockProvider.MockConfig?.IsMethodMocked(methodName) == true
		);
	}

	protected bool ShouldUseMockForProperty(string propertyName)
	{
		return MockProvider.Current != null &&
		(
			MockProvider.MockConfig == null ||
			MockProvider.MockConfig?.IsPropertyMocked(propertyName) == true
		);
	}
} 