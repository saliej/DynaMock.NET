using System;
using DynaMock.UnitTests.TestDoubles;
using DynaMock.UnitTests.TestServices;
using AwesomeAssertions;
using NSubstitute;

namespace DynaMock.UnitTests;

public class MockableAbstractClassTests
{
    public MockableAbstractClassTests()
    {
        DefaultMockProvider<AbstractTestService>.RemoveMock();
    }

    [Fact]
    public void Should_UseRealImplementation_WhenNoMockSet()
    {
        // Arrange
        var realImpl = Substitute.For<AbstractTestService>();
        realImpl.GetValue().Returns("Real Value");
        var mockable = new MockableAbstractTestServiceDouble(realImpl);

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
        var realImpl = Substitute.For<AbstractTestService>();
        realImpl.GetValue().Returns("Real Value");
        var mockable = new MockableAbstractTestServiceDouble(realImpl);

        var mock = Substitute.For<AbstractTestService>();
        mock.GetValue().Returns("Mocked Value");

		// Act
		DefaultMockProvider<AbstractTestService>.SetMock(mock);
        var result = mockable.GetValue();

        // Assert
        result.Should().Be("Mocked Value");
        realImpl.DidNotReceive().GetValue();
        mock.Received(1).GetValue();
    }

    [Fact]
    public void Should_CallVirtualMethod_OnMock()
    {
        // Arrange
        var realImpl = Substitute.For<AbstractTestService>();
        realImpl.GetDefaultValue().Returns("Real Default");
        var mockable = new MockableAbstractTestServiceDouble(realImpl);

        var mock = Substitute.For<AbstractTestService>();
        mock.GetDefaultValue().Returns("Mocked Default");

		// Act
		DefaultMockProvider<AbstractTestService>.SetMock(mock);
        var result = mockable.GetDefaultValue();

        // Assert
        result.Should().Be("Mocked Default");
        realImpl.DidNotReceive().GetDefaultValue();
        mock.Received(1).GetDefaultValue();
    }
}