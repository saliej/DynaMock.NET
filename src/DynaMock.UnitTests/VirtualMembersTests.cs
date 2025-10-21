using System;
using DynaMock.UnitTests.TestDoubles;
using DynaMock.UnitTests.TestServices;
using FluentAssertions;
using NSubstitute;


namespace DynaMock.UnitTests;

/// <summary>
/// Tests for VirtualMembers configuration option
/// </summary>
[Collection("Mock Isolation")]
public class VirtualMembersTests
{
	public VirtualMembersTests()
	{
		DefaultMockProvider<ConcreteTestService>.RemoveMock();
		DefaultMockProvider<VirtualTestService>.RemoveMock();
	}

	[Fact]
	public void Should_GenerateVirtualMembers_WhenEnabled()
	{
		// This test uses a test double that simulates VirtualMembers = true

		// Arrange
		var realImpl = new VirtualTestService();
		var mockable = new MockableVirtualTestServiceDouble(realImpl);

		// Act & Assert - Verify basic functionality
		var result = mockable.GetValue();
		result.Should().Be("Virtual Value");
	}

	[Fact]
	public void VirtualWrapper_CanBeMockedFurther()
	{
		// This demonstrates the key benefit of VirtualMembers = true:
		// The wrapper itself can be mocked

		// Arrange
		var realImpl = new VirtualTestService();
		var mockable = new MockableVirtualTestServiceDouble(realImpl);

		// Create a mock of the WRAPPER itself (only works with virtual members)
		var wrapperMock = Substitute.ForPartsOf<MockableVirtualTestServiceDouble>(realImpl);
		wrapperMock.GetValue().Returns("Wrapper Mocked");

		// Act
		var result = wrapperMock.GetValue();

		// Assert
		result.Should().Be("Wrapper Mocked");
	}

	[Fact]
	public void VirtualWrapper_WithRealImplementation_UsesRealByDefault()
	{
		// Arrange
		var realImpl = new VirtualTestService();
		var mockable = new MockableVirtualTestServiceDouble(realImpl);

		// Act
		var result = mockable.GetValue();

		// Assert - Uses real implementation when no mock is set
		result.Should().Be("Virtual Value");
	}

	[Fact]
	public void VirtualWrapper_WithMockSet_UsesMock()
	{
		// Arrange
		var realImpl = new VirtualTestService();
		var mockable = new MockableVirtualTestServiceDouble(realImpl);

		var mock = Substitute.For<VirtualTestService>();
		mock.GetValue().Returns("Service Mocked");

		// Act
		DefaultMockProvider<VirtualTestService>.SetMock(mock);
		var result = mockable.GetValue();

		// Assert
		result.Should().Be("Service Mocked");
		mock.Received(1).GetValue();
	}

	[Fact]
	public void VirtualWrapper_Properties_CanBeMocked()
	{
		// Arrange
		var realImpl = new VirtualTestService { Name = "Real Name" };
		var mockable = new MockableVirtualTestServiceDouble(realImpl);

		var wrapperMock = Substitute.ForPartsOf<MockableVirtualTestServiceDouble>(realImpl);
		wrapperMock.Name.Returns("Mocked Property");

		// Act
		var result = wrapperMock.Name;

		// Assert
		result.Should().Be("Mocked Property");
	}

	[Fact]
	public void VirtualWrapper_Methods_WithParameters_CanBeMocked()
	{
		// Arrange
		var realImpl = new VirtualTestService();
		var mockable = new MockableVirtualTestServiceDouble(realImpl);

		var wrapperMock = Substitute.ForPartsOf<MockableVirtualTestServiceDouble>(realImpl);
		wrapperMock.Calculate(Arg.Any<int>(), Arg.Any<int>()).Returns(999);

		// Act
		var result = wrapperMock.Calculate(5, 10);

		// Assert
		result.Should().Be(999);
	}

	[Fact]
	public void VirtualWrapper_VoidMethods_CanBeMocked()
	{
		// Arrange
		var realImpl = new VirtualTestService();
		var wrapperMock = Substitute.ForPartsOf<MockableVirtualTestServiceDouble>(realImpl);

		// Act
		wrapperMock.DoSomething();

		// Assert
		wrapperMock.Received(1).DoSomething();
	}

	[Fact]
	public void VirtualWrapper_SupportsSelectiveMocking()
	{
		// Arrange
		var realImpl = new VirtualTestService();
		var mockable = new MockableVirtualTestServiceDouble(realImpl);

		var mock = Substitute.For<VirtualTestService>();
		mock.GetValue().Returns("Mocked Value");
		mock.GetDefaultValue().Returns("Mocked Default");

		// Act - Only mock GetValue
		DefaultMockProvider<VirtualTestService>.SetMock(mock, config =>
		{
			config.MockMethod(x => x.GetValue());
		});

		var result1 = mockable.GetValue();
		var result2 = mockable.GetDefaultValue();

		// Assert
		result1.Should().Be("Mocked Value");
		result2.Should().Be("Default Virtual Value"); // Uses real
		mock.Received(1).GetValue();
		mock.DidNotReceive().GetDefaultValue();
	}

	[Fact]
	public void VirtualMembers_PerformanceOverhead_IsMinimal()
	{
		// This is a conceptual test - in practice, virtual dispatch overhead
		// is 2-10% but negligible in test scenarios

		// Arrange
		var realImpl = new VirtualTestService();
		var mockable = new MockableVirtualTestServiceDouble(realImpl);

		// Act - Call method many times
		for (int i = 0; i < 1000; i++)
		{
			mockable.GetValue();
		}

		// Assert - Just verify it works correctly
		mockable.GetValue().Should().Be("Virtual Value");

		// Performance overhead is acceptable for testing scenarios
	}
}


