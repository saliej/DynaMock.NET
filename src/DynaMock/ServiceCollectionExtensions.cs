using System;
using Microsoft.Extensions.DependencyInjection;

namespace DynaMock;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMockable<T>(
        this IServiceCollection services,
        Action<MockableOptions>? configureOptions = null) 
        where T : class
    {
        var options = new MockableOptions();
        configureOptions?.Invoke(options);

        // Get existing registration
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor == null)
        {
            throw new InvalidOperationException(
                $"Service {typeof(T).Name} must be registered before calling AddMockable<{typeof(T).Name}>");
        }

        // Look for generated mockable type
        var mockableTypeName = $"DynaMock.Generated.Mockable{typeof(T).Name}";

        var mockableType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.FullName == mockableTypeName);

        if (mockableType == null)
        {
            throw new InvalidOperationException(
                $"No mockable wrapper found for type {typeof(T).Name}. " +
                "Ensure the type is marked with [Mockable] attribute and the source generator is properly configured.");
        }

		if (descriptor.ImplementationType is null)
            throw new InvalidOperationException(
                $"Service {typeof(T).Name} must be registered with an implementation type.");

		// Remove existing registration
		services.Remove(descriptor);
                
        // Add new registration with mockable wrapper
        services.Add(new ServiceDescriptor(
            typeof(T),
            sp =>
            {
                var realImpl = ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType!);
                return ActivatorUtilities.CreateInstance(sp, mockableType, new[] { realImpl });
            },
            descriptor.Lifetime));

        return services;
    }
}