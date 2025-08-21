using System;
using DynaMock.UnitTests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DynaMock.UnitTests;

[Mockable(typeof(ITestService))]
public class MockableTypes {}
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMockable_ShouldRegisterMockableWrapper()
    {
        var services = new ServiceCollection();
        services.AddTransient<ITestService, TestService>();

        services.AddMockable<ITestService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<ITestService>();

        service.Should().NotBeNull();
        service.Should().BeAssignableTo<MockableBase<ITestService>>();
    }

    [Fact]
    public void AddMockable_ShouldPreserveLifetime()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        services.AddMockable<ITestService>();
        var provider = services.BuildServiceProvider();
        var service1 = provider.GetService<ITestService>();
        var service2 = provider.GetService<ITestService>();

        service1.Should().BeSameAs(service2);
    }

    [Fact]
    public void AddMockable_ShouldThrow_WhenServiceNotRegistered()
    {
        var services = new ServiceCollection();

        var act = () => services.AddMockable<ITestService>();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be registered before calling AddMockable*");
    }
}