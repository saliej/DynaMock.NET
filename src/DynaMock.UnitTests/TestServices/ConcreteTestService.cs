namespace DynaMock.UnitTests.TestServices;

/// <summary>
/// Concrete test service for testing concrete class mocking
/// </summary>
public class ConcreteTestService
{
	public virtual string Name { get; set; } = "Concrete Service";
	public virtual int Count { get; set; }

	// Non-virtual method - can still be mocked via delegation
	public virtual string GetValue()
	{
		return "Concrete Value";
	}

	// Virtual method
	public virtual string GetDefaultValue()
	{
		return "Default Value";
	}

	// Non-virtual method to test delegation
	public virtual string GetNonVirtualValue()
	{
		return "Non-Virtual Value";
	}

	// Method with parameters
	public virtual int Calculate(int a, int b)
	{
		return a + b;
	}

	// Void method
	public virtual void DoSomething()
	{
		// Implementation
	}

	// Async method
	public virtual async Task<string> GetValueAsync()
	{
		await Task.Delay(10);
		return "Async Value";
	}

	// Method with multiple parameters of different types
	public virtual bool Validate(string input, int threshold)
	{
		return !string.IsNullOrEmpty(input) && input.Length >= threshold;
	}

	// Generic method
	public virtual T GetGeneric<T>(T defaultValue)
	{
		return defaultValue;
	}
}