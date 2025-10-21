using System;
using DynaMock.UnitTests.TestDoubles;
using DynaMock.UnitTests.TestServices;
using FluentAssertions;
using NSubstitute;

namespace DynaMock.UnitTests;

public class MockableInterfaceTests
{
    public MockableInterfaceTests()
    {
		DefaultMockProvider<ITestService>.RemoveMock();
	}

    [Fact]
    public void Should_UseRealImplementation_WhenNoMockSet()
    {
        // Arrange
        var realImpl = Substitute.For<ITestService>();
        realImpl.GetValue().Returns("Real Value");
        var mockable = new MockableITestServiceDouble(realImpl);

        // Act
        var result = mockable.GetValue();

        // Assert
        result.Should().Be("Real Value");
        realImpl.Received(1).GetValue();
    }

    [Fact]
    public void Should_UseMock_WhenMockIsSet()
    {
        // Arrange
        var realImpl = Substitute.For<ITestService>();
        realImpl.GetValue().Returns("Real Value");
        var mockable = new MockableITestServiceDouble(realImpl);

        var mock = Substitute.For<ITestService>();
        mock.GetValue().Returns("Mocked Value");

		// Act
		DefaultMockProvider<ITestService>.SetMock(mock);
        var result = mockable.GetValue();

        // Assert
        result.Should().Be("Mocked Value");
        realImpl.DidNotReceive().GetValue();
        mock.Received(1).GetValue();
    }

	[Fact]
    public void Should_RevertToRealImplementation_WhenMockIsRemoved()
    {
        // Arrange
        var realImpl = Substitute.For<ITestService>();
        realImpl.GetValue().Returns("Real Value");
        var mockable = new MockableITestServiceDouble(realImpl);

        var mock = Substitute.For<ITestService>();
        mock.GetValue().Returns("Mocked Value");

		// Act
		DefaultMockProvider<ITestService>.SetMock(mock);
		DefaultMockProvider<ITestService>.RemoveMock();
        var result = mockable.GetValue();

        // Assert
        result.Should().Be("Real Value");
        realImpl.Received(1).GetValue();
        mock.DidNotReceive().GetValue();
    }

    [Fact]
    public async Task Should_WorkWithAsyncMethods()
    {
        // Arrange
        var realImpl = Substitute.For<ITestService>();
        realImpl.GetCountAsync().Returns(42);
        var mockable = new MockableITestServiceDouble(realImpl);

        var mock = Substitute.For<ITestService>();
        mock.GetCountAsync().Returns(99);

		// Act
		DefaultMockProvider<ITestService>.SetMock(mock);
        var result = await mockable.GetCountAsync();

        // Assert
        result.Should().Be(99);
        await mock.Received(1).GetCountAsync();
    }

    [Fact]
    public void Should_WorkWithProperties()
    {
        // Arrange
        var realImpl = Substitute.For<ITestService>();
        var mockable = new MockableITestServiceDouble(realImpl);

        var mock = Substitute.For<ITestService>();
		DefaultMockProvider<ITestService>.SetMock(mock);

        // Act
        mockable.Name = "Test";
        var result = mockable.Name;

        // Assert
        mock.Received(1).Name = "Test";
    }

	[Fact]
	public void Should_WorkWithEvents()
	{
		var realImpl = Substitute.For<ITestService>();
		var mockable = new MockableITestServiceDouble(realImpl);

		var mock = Substitute.For<ITestService>();
		DefaultMockProvider<ITestService>.SetMock(mock);

		var eventRaised = false;
		EventHandler handler = (s, e) => eventRaised = true;

		mockable.ValueChanged += handler;
		mock.ValueChanged += Raise.Event<EventHandler>(mock, EventArgs.Empty);

		eventRaised.Should().BeTrue();
	}
}