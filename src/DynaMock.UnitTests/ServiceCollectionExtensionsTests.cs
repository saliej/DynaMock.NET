using System;
using DynaMock.UnitTests.TestServices;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

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
	public void InjectedValue_Should_Be_OfType_MockableBase()
	{
		var services = new ServiceCollection();
		services.AddSingleton<ITestService, TestService>();

		services.AddMockable<ITestService>();
		var provider = services.BuildServiceProvider();
		var service = provider.GetService<ITestService>();
		var mockableBase = service as MockableBase<ITestService>;

		var mock = Substitute.For<ITestService>();
        mock.GetValue().Returns(_ =>
        {
            return "[Before Call] " + mockableBase.GetRealImplementation().GetValue() + " [After Call]";
		});

        var result = mock.GetValue();
        result.Should().Be("[Before Call] Test [After Call]");
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