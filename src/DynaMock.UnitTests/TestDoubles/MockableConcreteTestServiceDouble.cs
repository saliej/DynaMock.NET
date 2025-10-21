using DynaMock;
using DynaMock.UnitTests.TestServices;

namespace DynaMock.UnitTests.TestDoubles;

/// <summary>
/// Test double that simulates what the source generator would create for ConcreteTestService
/// This demonstrates the delegation pattern for concrete classes
/// </summary>
public class MockableConcreteTestServiceDouble : MockableBase<ConcreteTestService>
{
	// Single-parameter constructor (for ForPartsOf compatibility)
	public MockableConcreteTestServiceDouble(ConcreteTestService realImplementation)
		: this(realImplementation, null)
	{
	}

	// Two-parameter constructor (for explicit provider injection)
	public MockableConcreteTestServiceDouble(
		ConcreteTestService realImplementation,
		IMockProvider<ConcreteTestService>? mockProvider)
		: base(realImplementation, mockProvider)
	{
	}

	public string Name
	{
		get
		{
			if (ShouldUseMockForProperty(nameof(Name)))
				return MockProvider.Current.Name;

			return RealImplementation.Name;
		}
		set
		{
			if (ShouldUseMockForProperty(nameof(Name)))
			{
				MockProvider.Current.Name = value;
				return;
			}

			RealImplementation.Name = value;
		}
	}

	public int Count
	{
		get
		{
			if (ShouldUseMockForProperty(nameof(Count)))
				return MockProvider.Current.Count;

			return RealImplementation.Count;
		}
		set
		{
			if (ShouldUseMockForProperty(nameof(Count)))
			{
				MockProvider.Current.Count = value;
				return;
			}

			RealImplementation.Count = value;
		}
	}

	public string GetValue()
	{
		if (ShouldUseMockForMethod(nameof(GetValue)))
		{
			return MockProvider.Current.GetValue();
		}

		return RealImplementation.GetValue();
	}

	public string GetDefaultValue()
	{
		if (ShouldUseMockForMethod(nameof(GetDefaultValue)))
		{
			return MockProvider.Current.GetDefaultValue();
		}

		return RealImplementation.GetDefaultValue();
	}

	public string GetNonVirtualValue()
	{
		if (ShouldUseMockForMethod(nameof(GetNonVirtualValue)))
		{
			return MockProvider.Current.GetNonVirtualValue();
		}

		return RealImplementation.GetNonVirtualValue();
	}

	public int Calculate(int a, int b)
	{
		if (ShouldUseMockForMethod(nameof(Calculate)))
		{
			return MockProvider.Current.Calculate(a, b);
		}

		return RealImplementation.Calculate(a, b);
	}

	public void DoSomething()
	{
		if (ShouldUseMockForMethod(nameof(DoSomething)))
		{
			MockProvider.Current.DoSomething();
			return;
		}

		RealImplementation.DoSomething();
	}

	public async Task<string> GetValueAsync()
	{
		if (ShouldUseMockForMethod(nameof(GetValueAsync)))
		{
			return await MockProvider.Current.GetValueAsync();
		}

		return await RealImplementation.GetValueAsync();
	}

	public bool Validate(string input, int threshold)
	{
		if (ShouldUseMockForMethod(nameof(Validate)))
		{
			return MockProvider.Current.Validate(input, threshold);
		}

		return RealImplementation.Validate(input, threshold);
	}

	public T GetGeneric<T>(T defaultValue)
	{
		if (ShouldUseMockForMethod(nameof(GetGeneric)))
		{
			return MockProvider.Current.GetGeneric(defaultValue);
		}

		return RealImplementation.GetGeneric(defaultValue);
	}
}