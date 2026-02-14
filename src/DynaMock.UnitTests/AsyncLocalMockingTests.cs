using AwesomeAssertions;
using DynaMock.UnitTests.Services;
using NSubstitute;

namespace DynaMock.UnitTests;

[Collection("Mock Isolation")]
public class AsyncLocalMockingTests
{
	[Fact]
	public async Task AsyncLocal_ShouldIsolateThreads()
	{
		var realImpl = new BasicServiceImpl();
		var provider = new AsyncLocalMockProvider<IBasicService>();
		var service = DynaMockFactory.Create(realImpl, provider);

		var mock1 = Substitute.For<IBasicService>();
		mock1.GetValue().Returns("Mock1 Value");

		var mock2 = Substitute.For<IBasicService>();
		mock2.GetValue().Returns("Mock2 Value");

		var thread1Result = "";
		var thread2Result = "";
		var mainThreadResult = "";

		mainThreadResult = service.GetValue();

		var thread1 = new Thread(() =>
		{
			provider.SetMock(mock1);
			Thread.Sleep(100);
			thread1Result = service.GetValue();
		});

		var thread2 = new Thread(() =>
		{
			provider.SetMock(mock2);
			Thread.Sleep(100);
			thread2Result = service.GetValue();
		});

		thread1.Start();
		thread2.Start();
		thread1.Join();
		thread2.Join();

		mainThreadResult.Should().Be("RealGetValue");
		thread1Result.Should().Be("Mock1 Value");
		thread2Result.Should().Be("Mock2 Value");
	}

	[Fact]
	public async Task AsyncLocal_ShouldFlowThroughAsyncCalls()
	{
		var realImpl = new BasicServiceImpl();
		var provider = new AsyncLocalMockProvider<IBasicService>();
		var service = DynaMockFactory.Create(realImpl, provider);

		var mock = Substitute.For<IBasicService>();
		mock.Add(Arg.Any<int>(), Arg.Any<int>()).Returns(42);

		provider.SetMock(mock);
		var result = await GetCountThroughMultipleAsyncCalls(service);

		result.Should().Be(42);
	}

	[Fact]
	public async Task AsyncLocal_ShouldIsolateParallelAsyncTasks()
	{
		var realImpl = new BasicServiceImpl();
		var provider = new AsyncLocalMockProvider<IBasicService>();
		var service = DynaMockFactory.Create(realImpl, provider);

		var mock1 = Substitute.For<IBasicService>();
		mock1.Add(Arg.Any<int>(), Arg.Any<int>()).Returns(42);

		var mock2 = Substitute.For<IBasicService>();
		mock2.Add(Arg.Any<int>(), Arg.Any<int>()).Returns(99);

		var task1 = Task.Run(async () =>
		{
			provider.SetMock(mock1);
			await Task.Delay(50);
			return service.Add(1, 2);
		});

		var task2 = Task.Run(async () =>
		{
			provider.SetMock(mock2);
			await Task.Delay(50);
			return service.Add(3, 4);
		});

		var results = await Task.WhenAll(task1, task2);

		results[0].Should().Be(42);
		results[1].Should().Be(99);
	}

	[Fact]
	public void RemoveMock_ShouldClearAsyncLocalState()
	{
		var realImpl = new BasicServiceImpl();
		var provider = new AsyncLocalMockProvider<IBasicService>();
		var service = DynaMockFactory.Create(realImpl, provider);

		var mock = Substitute.For<IBasicService>();
		mock.GetValue().Returns("Mocked Value");

		provider.SetMock(mock);
		service.GetValue().Should().Be("Mocked Value");

		provider.RemoveMock();
		service.GetValue().Should().Be("RealGetValue");
	}

	private class BasicServiceImpl : IBasicService
	{
		public int Count { get; set; }
		public event EventHandler? OnBasicEvent;
		public event EventHandler<MyEventArgs>? OnEventWithArgs;

		public void DoSomething() { }
		public string GetValue() => "RealGetValue";
		public int Add(int first, int second) => first + second;
		public void RaiseBasicEvent() => OnBasicEvent?.Invoke(this, EventArgs.Empty);
		public void RaiseEventWithArgs(MyEventArgs myEventArgs) => OnEventWithArgs?.Invoke(this, myEventArgs);
	}

	private async Task<int> GetCountThroughMultipleAsyncCalls(IBasicService service)
	{
		await Task.Delay(10);
		var result = service.Add(1, 2);
		await Task.Delay(10);
		return result;
	}
}
