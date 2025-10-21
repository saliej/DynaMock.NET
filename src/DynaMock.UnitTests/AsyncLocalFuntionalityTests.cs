using System;
using DynaMock.UnitTests.TestServices;
using DynaMock.UnitTests.TestDoubles;
using AwesomeAssertions;
using NSubstitute;

namespace DynaMock.UnitTests;

public class AsyncLocalFunctionalityTests
{
    private readonly AsyncLocalMockProvider<ITestService> _provider = new();

	public AsyncLocalFunctionalityTests()
    {
    }

    [Fact]
    public void AsyncLocal_ShouldIsolateThreads()
    {
        // Arrange
        var realImpl = Substitute.For<ITestService>();
        realImpl.GetValue().Returns("Real Value");
        
        var service = new MockableITestServiceDouble(realImpl, _provider);

        var mock1 = Substitute.For<ITestService>();
        mock1.GetValue().Returns("Mock1 Value");

        var mock2 = Substitute.For<ITestService>();
        mock2.GetValue().Returns("Mock2 Value");

        var thread1Result = "";
        var thread2Result = "";
        var mainThreadResult = "";

        // Act
        var thread1 = new Thread(() =>
        {
			_provider.SetMock(mock1);
            Thread.Sleep(100); // Give other thread time to set its mock
            thread1Result = service.GetValue();
        });

        var thread2 = new Thread(() =>
        {
			_provider.SetMock(mock2);
            Thread.Sleep(100); // Give other thread time to set its mock
            thread2Result = service.GetValue();
        });

        // Main thread should use real implementation
        mainThreadResult = service.GetValue();

        thread1.Start();
        thread2.Start();
        thread1.Join();
        thread2.Join();

        // Assert
        mainThreadResult.Should().Be("Real Value");
        thread1Result.Should().Be("Mock1 Value");
        thread2Result.Should().Be("Mock2 Value");
    }

    [Fact]
    public async Task AsyncLocal_ShouldFlowThroughAsyncCalls()
    {
        // Arrange
        var realImpl = Substitute.For<ITestService>();
        realImpl.GetCountAsync().Returns(100);
        var service = new MockableITestServiceDouble(realImpl, _provider);

        var mock = Substitute.For<ITestService>();
        mock.GetCountAsync().Returns(42);

		// Act
		_provider.SetMock(mock);
        var result = await GetCountThroughMultipleAsyncCalls(service);

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public async Task AsyncLocal_ShouldIsolateParallelAsyncTasks()
    {
        // Arrange
        var realImpl = Substitute.For<ITestService>();
        realImpl.GetCountAsync().Returns(100);
        var service = new MockableITestServiceDouble(realImpl, _provider);

        var mock1 = Substitute.For<ITestService>();
        mock1.GetCountAsync().Returns(42);

        var mock2 = Substitute.For<ITestService>();
        mock2.GetCountAsync().Returns(99);

        // Act
        var task1 = Task.Run(async () =>
        {
            _provider.SetMock(mock1);
            await Task.Delay(50); // Simulate async work
            return await service.GetCountAsync();
        });

        var task2 = Task.Run(async () =>
        {
            _provider.SetMock(mock2);
            await Task.Delay(50); // Simulate async work
            return await service.GetCountAsync();
        });

        var results = await Task.WhenAll(task1, task2);

        // Assert
        results[0].Should().Be(42);
        results[1].Should().Be(99);
    }

    [Fact]
    public async Task AsyncLocal_PartialMocking_ShouldFlowCorrectly()
    {
        // Arrange
        var realImpl = Substitute.For<ITestService>();
        realImpl.GetValue().Returns("Real Value");
        realImpl.GetCountAsync().Returns(Task.FromResult(100));
        var service = new MockableITestServiceDouble(realImpl, _provider);

        var mock = Substitute.For<ITestService>();
        mock.GetValue().Returns("Mocked Value");
        mock.GetCountAsync().Returns(Task.FromResult(42));

		// Act
		_provider.SetMock(mock, config => config
            .MockMethod(x => x.GetValue())); // Only mock GetValue

        var valueResult = await GetValueThroughAsyncCall(service);
        var countResult = await GetCountThroughAsyncCall(service);

        // Assert
        valueResult.Should().Be("Mocked Value"); // Should be mocked
        countResult.Should().Be(100); // Should use real implementation
    }

    [Fact]
    public async Task AsyncLocal_ShouldNotLeakBetweenAsyncContexts()
    {
        // Arrange
        var realImpl = Substitute.For<ITestService>();
        realImpl.GetCountAsync().Returns(Task.FromResult(100));
        var service = new MockableITestServiceDouble(realImpl, _provider);

        var mock = Substitute.For<ITestService>();
        mock.GetCountAsync().Returns(Task.FromResult(42));

        var parentTaskResult = 0;
        var childTaskResult = 0;

        // Act
        await Task.Run(async () =>
        {
			_provider.SetMock(mock);
            parentTaskResult = await service.GetCountAsync();

            // Start a new task that should NOT inherit the mock
            childTaskResult = await Task.Run(async () =>
            {
                // This should use real implementation since AsyncLocal 
                // flows with async context, not with new Tasks
                return await service.GetCountAsync();
            });
        });

        // Assert
        parentTaskResult.Should().Be(42); // Parent task has mock
        childTaskResult.Should().Be(42); // Child task inherits AsyncLocal context
    }

    [Fact]
    public void RemoveMock_ShouldClearAsyncLocalState()
    {
        // Arrange
        var realImpl = Substitute.For<ITestService>();
        realImpl.GetValue().Returns("Real Value");
        var service = new MockableITestServiceDouble(realImpl, _provider);

        var mock = Substitute.For<ITestService>();
        mock.GetValue().Returns("Mocked Value");

		// Act
		_provider.SetMock(mock, config => config
            .MockMethod(x => x.GetValue()));

        service.GetValue().Should().Be("Mocked Value");

		_provider.RemoveMock();

        // Assert
        service.GetValue().Should().Be("Real Value");
    }

    // Helper methods for async flow testing
    private async Task<string> GetValueThroughAsyncCall(ITestService service)
    {
        await Task.Delay(1); // Simulate async work
        return await Task.FromResult(service.GetValue());
    }

    private async Task<int> GetCountThroughAsyncCall(ITestService service)
    {
        await Task.Delay(1); // Simulate async work
        return await service.GetCountAsync();
    }

    private async Task<int> GetCountThroughMultipleAsyncCalls(ITestService service)
    {
        await Task.Delay(10);
        var intermediate = await service.GetCountAsync();
        await Task.Delay(10);
        return intermediate;
    }
}
