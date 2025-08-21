using System;

namespace DynaMock;

public abstract class MockableBase<T> where T : class
{
    protected T _realImplementation;
    
    private static readonly AsyncLocal<T?> _mock = new();
    private static readonly AsyncLocal<MockConfiguration<T>?> _mockConfig = new();
    
    protected MockableBase(T realImplementation)
    {
        _realImplementation = realImplementation;
    }
    
    protected static T? Mock => _mock.Value;
    protected static MockConfiguration<T>? MockConfig => _mockConfig.Value;
    
    public static void SetMock(T? mockedService) =>
        SetMock(mockedService, null);
        
    public static void SetMock(T? mockedService, Action<MockConfiguration<T>>? configureOptions)
    {
        _mock.Value = mockedService;
        
        if (configureOptions != null && mockedService != null)
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
        
    public static void RemoveMock()
    {
        _mock.Value = null;
        _mockConfig.Value = null;
    }
    
    public static IDisposable SetScopedMock(T? mockedService, Action<MockConfiguration<T>>? configureOptions = null)
    {
        var previousMock = _mock.Value;
        var previousConfig = _mockConfig.Value;
        
        SetMock(mockedService, configureOptions);
        
        return new MockScope(() =>
        {
            _mock.Value = previousMock;
            _mockConfig.Value = previousConfig;
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