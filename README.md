# DynaMock.NET

A source generator library for creating mockable wrappers around interfaces and abstract classes, enabling partial mocking and switching between real implementations and mocks at runtime.

[![NuGet](https://img.shields.io/nuget/v/DynaMock.svg)](https://www.nuget.org/packages/DynaMock/)
[![Downloads](https://img.shields.io/nuget/dt/DynaMock.svg)](https://www.nuget.org/packages/DynaMock/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)


## Important: Design Philosophy & Intended Use

**This library is designed exclusively for testing scenarios and should not be used in production code.**

DynaMock.NET was created to address my (the author) specific needs when working with complex legacy codebases that lack proper dependency injection, testability, and clean architecture. It is **not** a replacement for good design principles and programming practices, or proper testing strategies.

**NB**: This package is currently considered stable for **my** needs, but certain features may be buggy, incomplete, or even be completely removed in future versions. I am not married to certain implementation and design choices and may change them for any reason, including the state of the weather on a particular day. I am also terrible at keeping documentation updated.

Things that are more likely to change than others:
- Mock registration process
- Adding/removing mocks
- Numerous internal implementations (e.g. interceptors)
- Testing methodologies

### Consider using this library if:

- You're working with legacy code that cannot be easily refactored
- You need to write tests for tightly-coupled dependencies
- You're dealing with static dependencies or singleton patterns
- You need partial mocking for integration testing scenarios
- Refactoring the relevant parts of the codebase is not feasible

### Do not use this library:

- As a substitute for proper interface design
- In production/runtime code
- As an excuse to avoid refactoring

### What you should be doing

Your code should have proper dependency injection, clear interfaces, and be designed for testability from the start. DynaMock.NET exists for the real world where various constraints make this ideal unattainable.

**Remember**: This library is a pragmatic tool for dealing with less-than-ideal circumstances. It is not a pattern to emulate in new code.

## Features

- **Partial Mocking**: Mock only specific methods/properties while using real implementations for others
- **Runtime Switching**: Toggle between real and mock implementations dynamically
- **Source Generated**: Zero runtime reflection overhead
- **Thread-Safe Options**: Choose between static or async-local mock providers
- **DI Integration**: First-class support for Microsoft.Extensions.DependencyInjection
- **Interface & Abstract Class Support**: Works with both interfaces and abstract classes
- **Event Support**: Mock events alongside methods and properties

## Installation

```bash
dotnet add package DynaMock
```

## Quick Start

### 1. Mark Types for Mocking

Use the `[Mockable]` attribute to specify which types should have mockable wrappers generated:

```csharp
using DynaMock;

[Mockable(typeof(IWeatherService))]
public class MockableTypes { }

public interface IWeatherService
{
    Task<string> GetWeatherAsync(string city);
    double GetTemperature();
    string Location { get; set; }
}
```

### 2. Register with Dependency Injection

```csharp
var services = new ServiceCollection();

// Register your service first
services.AddTransient<IWeatherService, WeatherService>();

// Add mockable wrapper
services.AddMockable<IWeatherService>();

var provider = services.BuildServiceProvider();
var service = provider.GetService<IWeatherService>(); // Returns mockable wrapper
```

### 3. Use in Tests

#### Full Mocking (All Methods Use Mock)

```csharp
// Arrange
var mock = Substitute.For<IWeatherService>();
mock.GetWeatherAsync("London").Returns("Sunny");
mock.GetTemperature().Returns(22.5);

// Set mock globally
DefaultMockProvider<IWeatherService>.SetMock(mock);

// Act
var weather = await service.GetWeatherAsync("London");
var temp = service.GetTemperature();

// Assert
Assert.Equal("Sunny", weather);
Assert.Equal(22.5, temp);

// Cleanup
DefaultMockProvider<IWeatherService>.RemoveMock();
```

#### Partial Mocking (Selective Methods)

```csharp
var realService = new WeatherService { Location = "London" };
var mock = Substitute.For<IWeatherService>();
mock.GetWeatherAsync(Arg.Any<string>()).Returns("Mocked Weather");

// Only mock GetWeatherAsync, use real implementation for everything else
DefaultMockProvider<IWeatherService>.SetMock(mock, config => config
    .MockMethod(x => x.GetWeatherAsync(default!)));

var weather = await service.GetWeatherAsync("London"); // Returns "Mocked Weather"
var temp = service.GetTemperature(); // Uses real implementation
var location = service.Location; // Uses real implementation
```

## Advanced Usage

### Thread-Safe Mocking with AsyncLocal

For parallel tests or multi-threaded scenarios:

```csharp
var provider = new AsyncLocalMockProvider<IWeatherService>();
var service = DynaMockFactory.Create(new WeatherService(), provider);

// Each async context gets isolated mock state
await Task.Run(async () =>
{
    var mock1 = Substitute.For<IWeatherService>();
    mock1.GetTemperature().Returns(25.0);
    provider.SetMock(mock1);
    
    var temp = service.GetTemperature(); // Returns 25.0
});

await Task.Run(async () =>
{
    var mock2 = Substitute.For<IWeatherService>();
    mock2.GetTemperature().Returns(30.0);
    provider.SetMock(mock2);
    
    var temp = service.GetTemperature(); // Returns 30.0
});
```

### Mocking Properties

```csharp
var mock = Substitute.For<IWeatherService>();
mock.Location.Returns("Paris");

DefaultMockProvider<IWeatherService>.SetMock(mock, config => config
    .MockProperty(x => x.Location));

service.Location = "Berlin"; // Sets on mock
var location = service.Location; // Returns "Paris" from mock
```

### Mocking Events

```csharp
var mock = Substitute.For<IWeatherService>();

DefaultMockProvider<IWeatherService>.SetMock(mock, config => config
    .MockEvent("WeatherChanged"));

var eventRaised = false;
service.WeatherChanged += (s, e) => eventRaised = true;

// Raise event on mock
mock.WeatherChanged += Raise.Event<EventHandler>(mock, EventArgs.Empty);

Assert.True(eventRaised);
```

### Virtual Members Option

Generate virtual methods/properties in the wrapper itself for additional flexibility:

```csharp
[Mockable(typeof(IMyService), VirtualMembers = true)]
public class MockableTypes { }
```

This allows the wrapper itself to be further mocked or inherited, useful for advanced testing scenarios.

### Manual Instantiation

Without DI, create mockable instances directly:

```csharp
var realImpl = new WeatherService();
var service = DynaMockFactory.Create<IWeatherService>(
    realImpl, 
    new DefaultMockProvider<IWeatherService>()
);
```

## Configuration API

```csharp
DefaultMockProvider<T>.SetMock(mock, config => config
    .MockMethod(x => x.MethodName())           // Mock specific method
    .MockMethod(x => x.AsyncMethod())          // Mock async methods
    .MockProperty(x => x.PropertyName)         // Mock specific property
    .MockEvent("EventName"));                  // Mock specific event
```

## Access Real Implementation

Access the underlying real implementation even when mocking:

```csharp
var mockable = service as MockableBase<IWeatherService>;
var realImpl = mockable.GetRealImplementation();

// Use in mock setup for spy-like behavior
mock.GetWeatherAsync(Arg.Any<string>())
    .Returns(async x => 
    {
        var realResult = await realImpl.GetWeatherAsync(x.Arg<string>());
        return $"[MOCKED] {realResult}";
    });
```

## Supported Types

- Interfaces
- Abstract classes
- Generic types
- Async methods
- Properties (get/set)
- Events

## Requirements

- .NET 9.0 or later
- Compatible with any mocking framework (NSubstitute, Moq, FakeItEasy, etc.)

## License

MIT
