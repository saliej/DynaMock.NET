using DynaMock.UnitTests.TestServices;
using Newtonsoft.Json.Linq;
using System;

namespace DynaMock.UnitTests.TestDoubles;

public class MockableITestServiceDouble : MockableBase<ITestService>, ITestService
{
	public MockableITestServiceDouble(ITestService realImplementation, 
		IMockProvider<ITestService>? mockProvider = null)
		: base(realImplementation, mockProvider)
	{
	}

	public string GetValue()
	{
		if (ShouldUseMockForMethod("GetValue"))
		{
			return MockProvider.Current.GetValue();
		}

		return RealImplementation.GetValue();
	}

	public System.Threading.Tasks.Task<int> GetCountAsync()
	{
		if (ShouldUseMockForMethod("GetCountAsync"))
		{
			return MockProvider.Current.GetCountAsync();
		}

		return RealImplementation.GetCountAsync();
	}

	public void SetValue(string value)
	{
		if (ShouldUseMockForMethod("SetValue"))
		{
			MockProvider.Current.SetValue(value);
		}

		RealImplementation.SetValue(value);
	}

	public string Name
	{
		get
		{
			if (ShouldUseMockForProperty("Name"))
				return MockProvider.Current.Name;

			return RealImplementation.Name;
		}
		set
		{
			if (ShouldUseMockForProperty("Name"))
				MockProvider.Current.Name = value;

			RealImplementation.Name = value;
		}
	}

	public event System.EventHandler ValueChanged
	{
		add
		{
			if (ShouldUseMockForEvent(nameof(ValueChanged)))
			{
				MockProvider.Current.ValueChanged += value;
				return;
			}
			RealImplementation.ValueChanged += value;
		}
		remove
		{
			if (ShouldUseMockForEvent(nameof(ValueChanged)))
			{
				MockProvider.Current.ValueChanged -= value;
				return;
			}
			RealImplementation.ValueChanged -= value;
		}
	}
}