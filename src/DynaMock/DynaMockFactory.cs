using System;

namespace DynaMock;

public static class DynaMockFactory
{
	public static T Create<T>(T realImplementation, IMockProvider<T> mockProvider) where T : class
	{
		// Find the generated mockable type using reflection, just like in ServiceCollectionExtensions.
		var mockableTypeName = $"DynaMock.Generated.Mockable{typeof(T).Name}";
		var mockableType = typeof(T).Assembly.GetType(mockableTypeName, throwOnError: true)
			?? throw new NullReferenceException($"Type {mockableTypeName} not found");

		return (T)Activator.CreateInstance(mockableType, realImplementation, mockProvider)!;
	}
}