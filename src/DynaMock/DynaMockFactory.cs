using System;

namespace DynaMock;

public static class DynaMockFactory
{
	public static T Create<T>(T realImplementation, IMockProvider<T> mockProvider) where T : class
	{
		// Find the generated mockable type using reflection, just like in ServiceCollectionExtensions.
		var mockableTypeName = $"DynaMock.Generated.Mockable{typeof(T).Name}";
		var mockableType = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(a => a.GetTypes())
			.FirstOrDefault(t => t.FullName == mockableTypeName)
			?? throw new InvalidOperationException($"No mockable wrapper found for type {typeof(T).Name}. Ensure the type is marked with [Mockable] attribute.");

		return (T)Activator.CreateInstance(mockableType, realImplementation, mockProvider)!;
	}
}