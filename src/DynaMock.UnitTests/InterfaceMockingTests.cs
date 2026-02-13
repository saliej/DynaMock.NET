using System;
using AwesomeAssertions;
using DynaMock.UnitTests.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DynaMock.UnitTests;

[Collection("Mock Isolation")]
public class InterfaceMockingTests : IDisposable
{
    public class BasicService : IBasicService
    {
        public void DoSomething()
        {
            throw new NotImplementedException();
        }

        public string GetValue() => "RealGetValue";

        public int Add(int first, int second) => first + second;
    
        public int Count { get; set; } = 5;
        public event EventHandler? OnBasicEvent;
        public event EventHandler<MyEventArgs>? OnEventWithArgs;
        
        public void RaiseBasicEvent() =>       
            OnBasicEvent?.Invoke(this, EventArgs.Empty);

        public void RaiseEventWithArgs(MyEventArgs myEventArgs) =>
            OnEventWithArgs?.Invoke(this, myEventArgs);
    }

    public void Dispose()
    {
        DefaultMockProvider<IBasicService>.RemoveMock();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void DynaMock_When_MethodCallConfigured_Should_Call_MockedMethod()
    {
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();
        
        var mock = Substitute.For<IBasicService>();
        mock.GetValue().Returns("MockedGetValue");

        DefaultMockProvider<IBasicService>.SetMock(mock, config => config.MockMethod(m => m.GetValue()));
        service.GetValue().Should().Be("MockedGetValue");

        // Should call the real implementation for everything else
        service.Add(1, 2).Should().Be(3);
        service.Count.Should().Be(5);

        var act = () => service.DoSomething();
        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void DynaMock_When_VoidMethodCallConfigured_Should_Call_MockedVoidMethod()
    {
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        var mock = Substitute.For<IBasicService>();
        // Do nothing, do not throw
        mock.When(m => m.DoSomething()).Do(_ => { });

        DefaultMockProvider<IBasicService>.SetMock(mock, config => config.MockMethod(m => m.DoSomething()));

        var act = () => service.DoSomething();
        act.Should().NotThrow();

        // Should call the real implementation for everything else
        service.GetValue().Should().Be("RealGetValue");
        service.Add(1, 2).Should().Be(3);
        service.Count.Should().Be(5);
    }

    [Fact]
    public void DynaMock_When_PropertyCallConfigured_Should_Call_MockedProperty()
    {
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        var mock = Substitute.For<IBasicService>();
        mock.Count.Returns(-10);

        DefaultMockProvider<IBasicService>.SetMock(mock, config => config.MockProperty(m => m.Count));

        // Should call the real implementation for everything else
        service.GetValue().Should().Be("RealGetValue");
        service.Add(1, 2).Should().Be(3);
        
        var act = () => service.DoSomething();
        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void DynaMock_Should_Call_Mock_By_Default()
    {
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        
        var mock = Substitute.For<IBasicService>();
        mock.Add(Arg.Any<int>(), Arg.Any<int>()).Returns(10);
        mock.GetValue().Returns("MockedGetValue");
        mock.Count.Returns(-10);

        // Do nothing, do not throw
        mock.When(m => m.DoSomething()).Do(_ => { });

        DefaultMockProvider<IBasicService>.SetMock(mock);
        var service = provider.GetRequiredService<IBasicService>();

        service.Add(1, 2).Should().Be(10);
        service.GetValue().Should().Be("MockedGetValue");
        service.Count.Should().Be(-10);

        var act = () => service.DoSomething();
        act.Should().NotThrow();
    }

    [Fact]
    public void DynaMock_Should_Respect_Mocking_Framework_Argument_Matchers()
    {
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        
        var mock = Substitute.For<IBasicService>();
        mock.Add(6, 5).Returns(10);

        // Do nothing, do not throw
        mock.When(m => m.DoSomething()).Do(_ => { });

        DefaultMockProvider<IBasicService>.SetMock(mock);
        var service = provider.GetRequiredService<IBasicService>();

        // Redirects to the mock, but the mock isn't configured for these parameters so 0 is returned
        service.Add(1, 2).Should().Be(0);
        service.Add(6, 5).Should().Be(10);
    }

    // Putting event tests here because they are messy
    [Fact]
    public void DynaMock_Should_Call_Mocked_Events_By_Default()
    {
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        var basicEventCalled = false;
        var eventArgValue = String.Empty;
        
        var mock = Substitute.For<IBasicService>();
        mock.OnBasicEvent += (_, _) => basicEventCalled = true;
        mock.OnEventWithArgs += (_, e) => eventArgValue = e.Value;

        DefaultMockProvider<IBasicService>.SetMock(mock);

        service.OnBasicEvent += Raise.Event<EventHandler>(null, EventArgs.Empty);
        service.OnEventWithArgs += Raise.Event<EventHandler<MyEventArgs>>(null, new MyEventArgs("test"));

        basicEventCalled.Should().BeTrue();
        eventArgValue.Should().Be("test");
    }

    [Fact]
    public void DynaMock_When_EventCallConfigured_Should_Call_MockedEvent()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IBasicService, BasicService>(); // Singleton specifically for this test
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();
        var realService = ((IMockableBase<IBasicService>)provider.GetRequiredService<IBasicService>())
            .GetRealImplementation();

        var basicEventCalled = false;
        var eventArgValue = String.Empty;
        
        var mock = Substitute.For<IBasicService>();
        mock.OnBasicEvent += (_, _) => basicEventCalled = true;
        mock.OnEventWithArgs += (_, e) => eventArgValue = $"mocked-{e.Value}";
        realService.OnEventWithArgs += (_, e) => eventArgValue = $"real-{e.Value}";

        DefaultMockProvider<IBasicService>.SetMock(mock, config => config.MockEvent("OnEventWithArgs"));

        service.OnBasicEvent += Raise.Event<EventHandler>(null, EventArgs.Empty);
        service.OnEventWithArgs += Raise.Event<EventHandler<MyEventArgs>>(null, new MyEventArgs("event"));

        try
        {
            basicEventCalled.Should().BeFalse(); // Not specified in mocking configuration
            eventArgValue.Should().Be("mocked-event"); 
        }
        finally
        {
            mock.OnBasicEvent -= null;
            realService.OnEventWithArgs -= null;
        }
    }
}