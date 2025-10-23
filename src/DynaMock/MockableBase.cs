//using System;
//using System.Reflection;

//namespace DynaMock;

//public abstract class MockableBase<T> where T : class
//{
//	protected readonly T RealImplementation;
//	protected readonly IMockProvider<T> MockProvider;

//	protected MockableBase(T realImplementation, IMockProvider<T>? mockProvider)
//	{
//		RealImplementation = realImplementation ?? throw new ArgumentNullException(nameof(realImplementation));

//		MockProvider = mockProvider ?? new DefaultMockProvider<T>();
//	}

//	protected T Implementation => MockProvider.Current ?? RealImplementation;

//	protected bool ShouldUseMockForMethod(string methodName)
//	{
//		return MockProvider.Current != null &&
//		(
//			MockProvider.MockConfig == null ||
//			MockProvider.MockConfig?.IsMethodMocked(methodName) == true
//		);
//	}

//	protected bool ShouldUseMockForProperty(string propertyName)
//	{
//		return MockProvider.Current != null &&
//		(
//			MockProvider.MockConfig == null ||
//			MockProvider.MockConfig?.IsPropertyMocked(propertyName) == true
//		);
//	}

//	protected bool ShouldUseMockForEvent(string eventName)
//	{
//		return MockProvider.Current != null &&
//		(
//			MockProvider.MockConfig == null ||
//			MockProvider.MockConfig?.IsEventMocked(eventName) == true
//		);
//	}

//	public T GetRealImplementation() => RealImplementation;
//}