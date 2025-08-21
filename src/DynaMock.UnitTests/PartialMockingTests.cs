using System;
using NSubstitute;

namespace DynaMock.UnitTests;

// Example service interface
public interface IAdvancedWeatherService
{
    Task<string> GetCurrentWeatherAsync();
    Task<T> GetDataAsync<T>(string endpoint);
    double GetTemperature();
    string Location { get; set; }
    bool IsOnline { get; }
    event EventHandler<string> WeatherChanged;
    event EventHandler ServiceStatusChanged;
}

// Real implementation
public class AdvancedWeatherService : IAdvancedWeatherService
{
    public string Location { get; set; } = "Default";
    public bool IsOnline => true;
    
    public event EventHandler<string>? WeatherChanged;
    public event EventHandler? ServiceStatusChanged;

    public async Task<string> GetCurrentWeatherAsync()
    {
        await Task.Delay(1000); // Simulate API call
        return "Sunny";
    }

    public async Task<T> GetDataAsync<T>(string endpoint)
    {
        await Task.Delay(500);
        return default(T)!;
    }

    public double GetTemperature() => 25.5;
    
    protected virtual void OnWeatherChanged(string weather)
    {
        WeatherChanged?.Invoke(this, weather);
    }
}

// Test examples showing partial mocking
public class PartialMockingExamples
{
    [Fact]
    public async Task FullMock_AllMethodsUseMock()
    {
        // Arrange
        var mock = Substitute.For<IAdvancedWeatherService>();
        mock.GetCurrentWeatherAsync().Returns("Mocked Weather");
        mock.GetTemperature().Returns(30.0);
        mock.Location.Returns("Mocked Location");
        
        // Act - No configuration means all methods use mock
        MockableAdvancedWeatherService.SetMock(mock);
        var service = new MockableAdvancedWeatherService(new AdvancedWeatherService());
        
        // Assert
        Assert.Equal("Mocked Weather", await service.GetCurrentWeatherAsync());
        Assert.Equal(30.0, service.GetTemperature());
        Assert.Equal("Mocked Location", service.Location);
    }
    
    [Fact]
    public async Task PartialMock_OnlySpecifiedMethodsUseMock()
    {
        // Arrange
        var realService = new AdvancedWeatherService { Location = "Real Location" };
        var mock = Substitute.For<IAdvancedWeatherService>();
        mock.GetCurrentWeatherAsync().Returns("Mocked Weather");
        mock.GetTemperature().Returns(30.0);
        
        var service = new MockableAdvancedWeatherService(realService);
        
        // Act - Only mock GetCurrentWeatherAsync, others use real implementation
        MockableAdvancedWeatherService.SetMock(mock, config => config
            .MockMethod(x => x.GetCurrentWeatherAsync()));
        
        // Assert
        Assert.Equal("Mocked Weather", await service.GetCurrentWeatherAsync()); // Mocked
        Assert.Equal(25.5, service.GetTemperature()); // Real implementation
        Assert.Equal("Real Location", service.Location); // Real implementation
    }
    
    [Fact]
    public async Task PartialMock_GenericMethods()
    {
        // Arrange
        var mock = Substitute.For<IAdvancedWeatherService>();
        mock.GetDataAsync<string>("test").Returns("Mocked Data");
        
        var service = new MockableAdvancedWeatherService(new AdvancedWeatherService());
        
        // Act - Mock the generic method
        MockableAdvancedWeatherService.SetMock(mock, config => config
            .MockMethod(x => x.GetDataAsync<string>("test"))); // This will match any GetDataAsync<T>
        
        // Assert
        Assert.Equal("Mocked Data", await service.GetDataAsync<string>("test"));
    }
    
    [Fact]
    public void PartialMock_Properties()
    {
        // Arrange
        var realService = new AdvancedWeatherService { Location = "Real Location" };
        var mock = Substitute.For<IAdvancedWeatherService>();
        mock.Location.Returns("Mocked Location");
        mock.IsOnline.Returns(false);
        
        var service = new MockableAdvancedWeatherService(realService);
        
        // Act - Only mock Location property
        MockableAdvancedWeatherService.SetMock(mock, config => config
            .MockProperty(x => x.Location));
        
        // Assert
        Assert.Equal("Mocked Location", service.Location); // Mocked
        Assert.True(service.IsOnline); // Real implementation
    }
    
    [Fact]
    public void PartialMock_MixedConfiguration()
    {
        // Arrange
        var realService = new AdvancedWeatherService { Location = "Real Location" };
        var mock = Substitute.For<IAdvancedWeatherService>();
        mock.GetTemperature().Returns(35.0);
        mock.Location.Returns("Mocked Location");
        
        var service = new MockableAdvancedWeatherService(realService);
        
        // Act - Mock multiple different member types
        MockableAdvancedWeatherService.SetMock(mock, config => config
            .MockMethod(x => x.GetTemperature())
            .MockProperty(x => x.Location));
        
        // Assert
        Assert.Equal(35.0, service.GetTemperature()); // Mocked
        Assert.Equal("Mocked Location", service.Location); // Mocked
        Assert.True(service.IsOnline); // Real implementation
        // GetCurrentWeatherAsync would use real implementation
    }
    
    [Fact]
    public void RemoveMock_ReturnsToFullRealImplementation()
    {
        // Arrange
        var realService = new AdvancedWeatherService { Location = "Real Location" };
        var mock = Substitute.For<IAdvancedWeatherService>();
        mock.GetTemperature().Returns(35.0);
        
        var service = new MockableAdvancedWeatherService(realService);
        
        // Act
        MockableAdvancedWeatherService.SetMock(mock, config => config
            .MockMethod(x => x.GetTemperature()));
        
        Assert.Equal(35.0, service.GetTemperature()); // Mocked
        
        MockableAdvancedWeatherService.RemoveMock();
        
        // Assert
        Assert.Equal(25.5, service.GetTemperature()); // Back to real implementation
    }
}

// Generated class would look like this:
public class MockableAdvancedWeatherService : MockableBase<IAdvancedWeatherService>, IAdvancedWeatherService
{
    public MockableAdvancedWeatherService(IAdvancedWeatherService realImplementation) 
        : base(realImplementation) { }

    public async Task<string> GetCurrentWeatherAsync()
    {
        if (ShouldUseMockForMethod("GetCurrentWeatherAsync"))
            return await Mock.GetCurrentWeatherAsync();
        return await _realImplementation.GetCurrentWeatherAsync();
    }

    public async Task<T> GetDataAsync<T>(string endpoint)
    {
        if (ShouldUseMockForMethod("GetDataAsync"))
            return await Mock.GetDataAsync<T>(endpoint);
        return await _realImplementation.GetDataAsync<T>(endpoint);
    }

    public double GetTemperature()
    {
        if (ShouldUseMockForMethod("GetTemperature"))
            return Mock.GetTemperature();
        return _realImplementation.GetTemperature();
    }

    public string Location
    {
        get
        {
            if (ShouldUseMockForProperty("Location"))
                return Mock.Location;
            return _realImplementation.Location;
        }
        set
        {
            if (ShouldUseMockForProperty("Location"))
                Mock.Location = value;
            else
                _realImplementation.Location = value;
        }
    }

    public bool IsOnline
    {
        get
        {
            if (ShouldUseMockForProperty("IsOnline"))
                return Mock.IsOnline;
            return _realImplementation.IsOnline;
        }
    }

    public event EventHandler<string> WeatherChanged
    {
        add => Implementation.WeatherChanged += value;
        remove => Implementation.WeatherChanged -= value;
    }

    public event EventHandler ServiceStatusChanged
    {
        add => Implementation.ServiceStatusChanged += value;
        remove => Implementation.ServiceStatusChanged -= value;
    }
}