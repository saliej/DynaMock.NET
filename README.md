# DynaMock.NET

A C# source generator library for creating mockable wrappers around interfaces and abstract classes. Designed to support runtime switching of mocks.

## Quick Start

1. Install the package:
```bash
dotnet add package DynaMock.NET
```

2. Mark types for generation:
```bash
TODO
```

3. Register in DI:
```bash
csharpservices.AddTransient<IWeatherService, WeatherService>();
services.AddMockable<IWeatherService>();
```

4. Use in tests:
```bash
TODO
```

5. Documentation
TODO

6. License
Apache 2.0 License - see LICENSE file for details.