using System;

namespace DynaMock;

/// <summary>
/// Marks a class as a mockable type declaration.
/// Specify the types to mock in the constructor.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
public class MockableAttribute : Attribute
{
	public List<Type> MockTypes { get; set; }

	/// <summary>
	/// When true, generates virtual methods and properties in the wrapper.
	/// This allows the generated wrapper itself to be mocked or inherited.
	/// Default: false (non-virtual for better performance)
	/// </summary>
	public bool VirtualMembers { get; set; } = false;

	public MockableAttribute(params Type[] mockTypes)
	{
		MockTypes = [.. mockTypes];
	}
}