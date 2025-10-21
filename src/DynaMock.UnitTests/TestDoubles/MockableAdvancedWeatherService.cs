using System;

namespace DynaMock.UnitTests.TestDoubles;

public class MockableAdvancedWeatherService : MockableBase<IAdvancedWeatherService>, IAdvancedWeatherService
{
	public MockableAdvancedWeatherService(IAdvancedWeatherService realImplementation, IMockProvider<IAdvancedWeatherService>? mockProvider = null)
		: base(realImplementation, mockProvider) { }

	public async Task<string> GetCurrentWeatherAsync()
	{
		if (ShouldUseMockForMethod("GetCurrentWeatherAsync"))
			return await MockProvider.Current.GetCurrentWeatherAsync();
		return await RealImplementation.GetCurrentWeatherAsync();
	}

	public async Task<T> GetDataAsync<T>(string endpoint)
	{
		if (ShouldUseMockForMethod("GetDataAsync"))
			return await MockProvider.Current.GetDataAsync<T>(endpoint);
		return await RealImplementation.GetDataAsync<T>(endpoint);
	}

	public double GetTemperature()
	{
		if (ShouldUseMockForMethod("GetTemperature"))
			return MockProvider.Current.GetTemperature();
		return RealImplementation.GetTemperature();
	}

	public string Location
	{
		get
		{
			if (ShouldUseMockForProperty("Location"))
				return MockProvider.Current.Location;
			return RealImplementation.Location;
		}
		set
		{
			if (ShouldUseMockForProperty("Location"))
				MockProvider.Current.Location = value;
			else
				RealImplementation.Location = value;
		}
	}

	public bool IsOnline
	{
		get
		{
			if (ShouldUseMockForProperty("IsOnline"))
				return MockProvider.Current.IsOnline;
			return RealImplementation.IsOnline;
		}
	}

	public event EventHandler<string> WeatherChanged
	{
		add
		{
			if (ShouldUseMockForEvent("WeatherChanged"))
			{
				MockProvider.Current.WeatherChanged += value;
				return;
			}
			RealImplementation.WeatherChanged += value;
		}
		remove
		{
			if (ShouldUseMockForEvent("WeatherChanged"))
			{
				MockProvider.Current.WeatherChanged -= value;
				return;
			}
			RealImplementation.WeatherChanged -= value;
		}
	}

	public event EventHandler ServiceStatusChanged
	{
		add
		{
			if (ShouldUseMockForEvent("ServiceStatusChanged"))
			{
				MockProvider.Current.ServiceStatusChanged += value;
				return;
			}
			RealImplementation.ServiceStatusChanged += value;
		}
		remove
		{
			if (ShouldUseMockForEvent("ServiceStatusChanged"))
			{
				MockProvider.Current.ServiceStatusChanged -= value;
				return;
			}
			RealImplementation.ServiceStatusChanged -= value;
		}
	}
}
