using System;
using DynaMock.UnitTests.TestServices;
//using DynaMock.UnitTests.TestDoubles;
using DynaMock.Generated;
using AwesomeAssertions;
using NSubstitute;

namespace DynaMock.UnitTests;

public class MockableConcreteClassTests
{
    public class DummyConcreteService : ConcreteService
    {
        public new string GetValue()
        {
            return "DummyValue";
        }

        public new int Add(int first, int second)
        {
            return -4;
        }
    }

    [Fact]
    public void Should_UseRealImplementation_WhenNoMockSet()
    {
        // Arrange
        var realImpl = new ConcreteService();
        var mockable = new MockableConcreteService(realImpl);

        var mock = new DummyConcreteService();
        DefaultMockProvider<ConcreteService>.SetMock(mock, config => config
            .MockMethod(x => x.Add(3, 5), mock));
        
        // Act
        mockable.Add(1, 3).Should().Be(4);
        mockable.Add(3, 5).Should().Be(-4);
    }
}

//[Collection("Mock Isolation")]
//public class MockableConcreteClassTests
//{
//	public MockableConcreteClassTests()
//	{
//		DefaultMockProvider<ConcreteTestService>.RemoveMock();
//	}

//	[Fact]
//	public void Should_UseRealImplementation_WhenNoMockSet()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService();
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		// Act
//		var result = mockable.GetValue();

//		// Assert
//		result.Should().Be("Concrete Value");
//	}

//	[Fact]
//	public void Should_UseMock_WhenMockIsSet()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService();
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		var mock = Substitute.For<ConcreteTestService>();
//		mock.GetValue().Returns("Mocked Value");

//		// Act
//		DefaultMockProvider<ConcreteTestService>.SetMock(mock);
//		var result = mockable.GetValue();

//		// Assert
//		result.Should().Be("Mocked Value");
//		mock.Received(1).GetValue();
//	}

//	[Fact]
//	public void Should_CallVirtualMethod_OnMock()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService();
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		var mock = Substitute.For<ConcreteTestService>();
//		mock.GetDefaultValue().Returns("Mocked Default");

//		// Act
//		DefaultMockProvider<ConcreteTestService>.SetMock(mock);
//		var result = mockable.GetDefaultValue();

//		// Assert
//		result.Should().Be("Mocked Default");
//		mock.Received(1).GetDefaultValue();
//	}

//	[Fact]
//	public void Should_MockNonVirtualMethod_ViaDelegation()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService();
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		var mock = Substitute.For<ConcreteTestService>();
//		mock.GetNonVirtualValue().Returns("Mocked Non-Virtual");

//		// Act
//		DefaultMockProvider<ConcreteTestService>.SetMock(mock);
//		var result = mockable.GetNonVirtualValue();

//		// Assert
//		result.Should().Be("Mocked Non-Virtual");
//		mock.Received(1).GetNonVirtualValue();
//	}

//	[Fact]
//	public void Should_MockProperties_OnConcreteClass()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService { Name = "Real Name" };
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		var mock = Substitute.For<ConcreteTestService>();
//		mock.Name.Returns("Mocked Name");

//		// Act
//		DefaultMockProvider<ConcreteTestService>.SetMock(mock);
//		var result = mockable.Name;

//		// Assert
//		result.Should().Be("Mocked Name");
//	}

//	[Fact]
//	public void Should_SwitchBetweenRealAndMock()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService();
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		var mock = Substitute.For<ConcreteTestService>();
//		mock.GetValue().Returns("Mocked Value");

//		// Act & Assert - Real
//		var result1 = mockable.GetValue();
//		result1.Should().Be("Concrete Value");

//		// Act & Assert - Mock
//		DefaultMockProvider<ConcreteTestService>.SetMock(mock);
//		var result2 = mockable.GetValue();
//		result2.Should().Be("Mocked Value");

//		// Act & Assert - Back to Real
//		DefaultMockProvider<ConcreteTestService>.RemoveMock();
//		var result3 = mockable.GetValue();
//		result3.Should().Be("Concrete Value");
//	}

//	[Fact]
//	public void Should_MockOnlySpecifiedMethods_WithConfiguration()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService();
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		var mock = Substitute.For<ConcreteTestService>();
//		mock.GetValue().Returns("Mocked Value");
//		mock.GetDefaultValue().Returns("Mocked Default");

//		// Act - Only mock GetValue
//		DefaultMockProvider<ConcreteTestService>.SetMock(mock, config =>
//		{
//			config.MockMethod(x => x.GetValue());
//		});

//		var result1 = mockable.GetValue();
//		var result2 = mockable.GetDefaultValue();

//		// Assert
//		result1.Should().Be("Mocked Value");
//		result2.Should().Be("Default Value"); // Uses real implementation
//		mock.Received(1).GetValue();
//		mock.DidNotReceive().GetDefaultValue();
//	}

//	[Fact]
//	public void Should_HandleMethodsWithParameters()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService();
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		var mock = Substitute.For<ConcreteTestService>();
//		mock.Calculate(Arg.Any<int>(), Arg.Any<int>()).Returns(999);

//		// Act
//		DefaultMockProvider<ConcreteTestService>.SetMock(mock);
//		var result = mockable.Calculate(5, 10);

//		// Assert
//		result.Should().Be(999);
//		mock.Received(1).Calculate(5, 10);
//	}

//	[Fact]
//	public void Should_HandleVoidMethods()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService();
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		var mock = Substitute.For<ConcreteTestService>();

//		// Act
//		DefaultMockProvider<ConcreteTestService>.SetMock(mock);
//		mockable.DoSomething();

//		// Assert
//		mock.Received(1).DoSomething();
//	}

//	[Fact]
//	public async Task Should_HandleAsyncMethods()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService();
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		var mock = Substitute.For<ConcreteTestService>();
//		mock.GetValueAsync().Returns(Task.FromResult("Mocked Async"));

//		// Act
//		DefaultMockProvider<ConcreteTestService>.SetMock(mock);
//		var result = await mockable.GetValueAsync();

//		// Assert
//		result.Should().Be("Mocked Async");
//		await mock.Received(1).GetValueAsync();
//	}

//	[Fact]
//	public void Should_Provide_Access_To_RealImplementation()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService();
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		var mock = Substitute.For<ConcreteTestService>();
//		mock.GetValue()
//			.Returns(_ =>
//			{ 
//				return "[Before Call]" + mockable.GetRealImplementation().GetValue() + "[After Call]";
//			});

//		// Act
//		DefaultMockProvider<ConcreteTestService>.SetMock(mock);

//		var result = mockable.GetValue();

//		// Assert
//		result.Should().Be("[Before Call]Concrete Value[After Call]");
//	}

//	[Fact]
//	public void Should_MockPropertyGetters()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService { Count = 42 };
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		var mock = Substitute.For<ConcreteTestService>();
//		mock.Count.Returns(100);

//		// Act
//		DefaultMockProvider<ConcreteTestService>.SetMock(mock, config =>
//		{
//			config.MockProperty(x => x.Count);
//		});
//		var result = mockable.Count;

//		// Assert
//		result.Should().Be(100);
//	}

//	[Fact]
//	public void Should_MockPropertySetters()
//	{
//		// Arrange
//		var realImpl = new ConcreteTestService();
//		var mockable = new MockableConcreteTestServiceDouble(realImpl);

//		var mock = Substitute.For<ConcreteTestService>();

//		// Act
//		DefaultMockProvider<ConcreteTestService>.SetMock(mock, config =>
//		{
//			config.MockProperty(x => x.Count);
//		});
//		mockable.Count = 99;

//		// Assert
//		mock.Received().Count = 99;
//		realImpl.Count.Should().Be(0); // Real implementation not called
//	}
//}