using System;
using AwesomeAssertions;
using DynaMock;
using DynaMock.UnitTests.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DynaMock.UnitTests;

[Collection("Mock Isolation")]
public class CallInterceptorTests : IDisposable
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
    public void InterceptMethod_WithNoMock_CallsRealImplementation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        // Act
        var result = service.GetValue();

        // Assert
        result.Should().Be("RealGetValue");
    }

    [Fact]
    public void InterceptMethod_WithMockButNoConfig_CallsMock()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        var mock = Substitute.For<IBasicService>();
        mock.GetValue().Returns("MockedGetValue");
        DefaultMockProvider<IBasicService>.SetMock(mock);

        // Act
        var result = service.GetValue();

        // Assert
        result.Should().Be("MockedGetValue");
    }

    [Fact]
    public void InterceptMethod_WithMockAndConfig_CallsMock()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        var mock = Substitute.For<IBasicService>();
        mock.GetValue().Returns("MockedGetValue");
        DefaultMockProvider<IBasicService>.SetMock(mock, config => config.MockMethod(x => x.GetValue()));

        // Act
        var result = service.GetValue();

        // Assert
        result.Should().Be("MockedGetValue");
    }

    [Fact]
    public void InterceptMethod_WithMockButConfigForOtherMethod_CallsReal()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        var mock = Substitute.For<IBasicService>();
        mock.GetValue().Returns("MockedGetValue");
        mock.Add(Arg.Any<int>(), Arg.Any<int>()).Returns(100);
        DefaultMockProvider<IBasicService>.SetMock(mock, config => config.MockMethod(x => x.Add(0, 0)));

        // Act
        var result = service.GetValue();

        // Assert
        result.Should().Be("RealGetValue");
        mock.DidNotReceive().GetValue();
    }

    [Fact]
    public void InterceptVoidMethod_WithMockButNoConfig_CallsMock()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        var mock = Substitute.For<IBasicService>();
        var didThrow = false;
        mock.When(x => x.DoSomething()).Do(_ => didThrow = true);
        DefaultMockProvider<IBasicService>.SetMock(mock);

        // Act
        var exception = Record.Exception(() => service.DoSomething());

        // Assert - Mock should be called, no exception thrown
        exception.Should().BeNull();
        didThrow.Should().BeTrue();
        mock.Received(1).DoSomething();
    }

    [Fact]
    public void InterceptPropertyGet_WithNoMock_CallsRealImplementation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        // Act
        var result = service.Count;

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void InterceptPropertyGet_WithMockButNoConfig_CallsMock()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        var mock = Substitute.For<IBasicService>();
        mock.Count.Returns(10);
        DefaultMockProvider<IBasicService>.SetMock(mock);

        // Act
        var result = service.Count;

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public void InterceptPropertyGet_WithMockAndConfig_CallsMock()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        var mock = Substitute.For<IBasicService>();
        mock.Count.Returns(20);
        DefaultMockProvider<IBasicService>.SetMock(mock, config => config.MockProperty(x => x.Count));

        // Act
        var result = service.Count;

        // Assert
        result.Should().Be(20);
    }

    [Fact]
    public void InterceptPropertySet_WithMockButNoConfig_CallsMock()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        var mock = Substitute.For<IBasicService>();
        int? capturedValue = null;
        mock.Count = Arg.Do<int>(x => capturedValue = x);
        DefaultMockProvider<IBasicService>.SetMock(mock);

        // Act
        service.Count = 15;

        // Assert
        capturedValue.Should().Be(15);
        mock.Received(1).Count = Arg.Any<int>();
    }

    [Fact]
    public void InterceptPropertySet_WithMockAndConfig_CallsMock()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        var mock = Substitute.For<IBasicService>();
        int? capturedValue = null;
        mock.Count = Arg.Do<int>(x => capturedValue = x);
        DefaultMockProvider<IBasicService>.SetMock(mock, config => config.MockProperty(x => x.Count));

        // Act
        service.Count = 25;

        // Assert
        capturedValue.Should().Be(25);
        mock.Received(1).Count = Arg.Any<int>();
    }

    [Fact]
    public void InterceptEventRemove_WithMockButNoConfig_CallsMock()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IBasicService, BasicService>();
        services.AddMockable<IBasicService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IBasicService>();

        var mock = Substitute.For<IBasicService>();
        EventHandler handler = (s, e) => { };
        mock.OnBasicEvent += handler;
        DefaultMockProvider<IBasicService>.SetMock(mock);

        // Act
        service.OnBasicEvent -= handler;

        // Assert
        mock.Received(1).OnBasicEvent -= handler;
    }
}
