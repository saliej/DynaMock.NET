using System;

namespace DynaMock;

public abstract class MockableBase<T> where T : class
{
    protected T _realImplementation;
    
    private static T? _mock;
    private static MockConfiguration<T>? _mockConfig = new();
    
    protected MockableBase(T realImplementation)
    {
        _realImplementation = realImplementation;
    }
    
    protected static T? Mock => _mock;
    protected static MockConfiguration<T>? MockConfig => _mockConfig;
    
    public static void SetMock(T? mockedService) =>
        SetMock(mockedService, null);
        
    public static void SetMock(T? mockedService, Action<MockConfiguration<T>>? configureOptions)
    {
        _mock = mockedService;
        
        if (configureOptions != null && mockedService != null)
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
        
    public static void RemoveMock()
    {
        _mock = null;
        _mockConfig = null;
    }
    
    public static IDisposable SetScopedMock(T? mockedService, Action<MockConfiguration<T>>? configureOptions = null)
    {
        var previousMock = _mock;
        var previousConfig = _mockConfig;
        
        SetMock(mockedService, configureOptions);
        
        return new MockScope(() =>
        {
            _mock = previousMock;
            _mockConfig = previousConfig;
        });
    }
    
    protected T Implementation => Mock ?? _realImplementation;
    
    // Helper methods for generated code
    protected bool ShouldUseMockForMethod(string methodName)
    {
        return Mock != null && (MockConfig?.IsMethodMocked(methodName) ?? true);
    }
    
    protected bool ShouldUseMockForProperty(string propertyName)
    {
        return Mock != null && (MockConfig?.IsPropertyMocked(propertyName) ?? true);
    }
}