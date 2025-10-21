using DynaMock.UnitTests.TestDoubles;
using FluentAssertions;
using NSubstitute;

namespace DynaMock.UnitTests;

public delegate void CustomEventHandler(object sender, string message);

public class DataEventArgs : EventArgs
{
	public string Data { get; set; } = string.Empty;
}

public class ServiceWithEvents
{
	public virtual event EventHandler? StatusChanged;
	public virtual event EventHandler<DataEventArgs>? DataReceived;
	public virtual event CustomEventHandler? CustomEvent;

	public virtual void RaiseStatusChanged()
	{
		StatusChanged?.Invoke(this, EventArgs.Empty);
	}

	public virtual void RaiseDataReceived(string data)
	{
		DataReceived?.Invoke(this, new DataEventArgs { Data = data });
	}

	public virtual void RaiseCustomEvent(string message)
	{
		CustomEvent?.Invoke(this, message);
	}
}

[Collection("Mock Isolation")]
public class EventMockingTests
{
	public EventMockingTests()
	{
		DefaultMockProvider<ServiceWithEvents>.RemoveMock();
	}

	[Fact]
	public void Should_SubscribeToRealEvent_WhenNoMockSet()
	{
		var realImpl = Substitute.For<ServiceWithEvents>();
		var wrapper = new MockableServiceWithEventsDouble(realImpl);

		var eventData = string.Empty;
		realImpl.StatusChanged += (s, e) => eventData = "Real Implementation";

		wrapper.StatusChanged += Raise.Event();

		eventData.Should().Be("Real Implementation");
	}

	[Fact]
	public void Should_SubscribeToMockEvent_WhenMockIsSet()
	{
		var realImpl = Substitute.For<ServiceWithEvents>();
		var wrapper = new MockableServiceWithEventsDouble(realImpl);
		
		var mock = Substitute.For<ServiceWithEvents>();
		DefaultMockProvider<ServiceWithEvents>.SetMock(mock);

		var eventData = string.Empty;
		realImpl.StatusChanged += (s, e) => eventData = "Real Implementation";
		wrapper.StatusChanged += (s, e) => eventData = "Mocked Implementation";

		mock.StatusChanged += Raise.Event<EventHandler>(mock, EventArgs.Empty);

		eventData.Should().Be("Mocked Implementation");
	}

	[Fact]
	public void Should_UnsubscribeFromMockEvent()
	{
		var realImpl = Substitute.For<ServiceWithEvents>();
		var mock = Substitute.For<ServiceWithEvents>();
		var wrapper = new MockableServiceWithEventsDouble(realImpl);

		DefaultMockProvider<ServiceWithEvents>.SetMock(mock);

		var eventCount = 0;
		EventHandler handler = (s, e) => eventCount++;

		mock.StatusChanged += handler;
		wrapper.StatusChanged += Raise.Event<EventHandler>(mock, EventArgs.Empty);
		eventCount.Should().Be(1);

		mock.StatusChanged -= handler;
		wrapper.StatusChanged += Raise.Event<EventHandler>(mock, EventArgs.Empty);
		eventCount.Should().Be(1);
	}

	[Fact]
	public void Should_Call_RealImplementation_When_MockIsRemoved()
	{
		var realImpl = Substitute.For<ServiceWithEvents>();
		var wrapper = new MockableServiceWithEventsDouble(realImpl);

		var mock = Substitute.For<ServiceWithEvents>();
		DefaultMockProvider<ServiceWithEvents>.SetMock(mock);

		var eventData = string.Empty;
		realImpl.StatusChanged += (s, e) => eventData = "Real Implementation";
		wrapper.StatusChanged += (s, e) => eventData = "Mocked Implementation";

		// Should raise event on mock
		wrapper.StatusChanged += Raise.Event<EventHandler>(mock, EventArgs.Empty);
		eventData.Should().Be("Mocked Implementation");

		// Remove mock and verify event goes to real implementation
		DefaultMockProvider<ServiceWithEvents>.RemoveMock();

		wrapper.StatusChanged += Raise.Event<EventHandler>(mock, EventArgs.Empty);
		eventData.Should().Be("Real Implementation");
	}

	[Fact]
	public void Should_HandleGenericEventArgs()
	{
		var realImpl = Substitute.For<ServiceWithEvents>();
		var mock = Substitute.For<ServiceWithEvents>();
		var wrapper = new MockableServiceWithEventsDouble(realImpl);

		DefaultMockProvider<ServiceWithEvents>.SetMock(mock);

		var receivedData = string.Empty;
		wrapper.DataReceived += (s, e) => receivedData = e.Data;

		mock.DataReceived += Raise.Event<EventHandler<DataEventArgs>>(
			mock,
			new DataEventArgs { Data = "Test Data" });

		receivedData.Should().Be("Test Data");
	}

	[Fact]
	public void Should_HandleCustomDelegates()
	{
		var realImpl = Substitute.For<ServiceWithEvents>();
		var mock = Substitute.For<ServiceWithEvents>();
		var wrapper = new MockableServiceWithEventsDouble(realImpl);

		DefaultMockProvider<ServiceWithEvents>.SetMock(mock);

		string receivedMessage = string.Empty;
		mock.CustomEvent += (s, m) => receivedMessage = m;

		wrapper.CustomEvent += Raise.Event<CustomEventHandler>(mock, "Custom Message");

		receivedMessage.Should().Be("Custom Message");
	}

	[Fact]
	public void Should_MockOnlySpecifiedEvents()
	{
		var realImpl = Substitute.For<ServiceWithEvents>();
		var mock = Substitute.For<ServiceWithEvents>();
		var wrapper = new MockableServiceWithEventsDouble(realImpl);

		DefaultMockProvider<ServiceWithEvents>.SetMock(mock, config =>
		{
			config.MockEvent("StatusChanged");
		});

		var statusChangedRaised = false;
		var dataReceivedRaised = false;

		mock.StatusChanged += (s, e) => statusChangedRaised = true;
		mock.DataReceived += (s, e) => dataReceivedRaised = true;

		wrapper.StatusChanged += Raise.Event<EventHandler>(mock, EventArgs.Empty);
		statusChangedRaised.Should().BeTrue();

		wrapper.DataReceived += Raise.Event<EventHandler<DataEventArgs>>(
			wrapper,
			new DataEventArgs { Data = "Test" });
		dataReceivedRaised.Should().BeFalse();
	}

	[Fact]
	public void Should_RouteSubscriptionsBasedOnActiveProvider()
	{
		var realImpl = Substitute.For<ServiceWithEvents>();
		var mock = Substitute.For<ServiceWithEvents>();
		var wrapper = new MockableServiceWithEventsDouble(realImpl);

		var realEventCount = 0;
		var mockEventCount = 0;

		// Subscribe while real is active
		EventHandler realHandler = (s, e) => realEventCount++;
		realImpl.StatusChanged += realHandler;

		wrapper.StatusChanged += Raise.Event<EventHandler>(realImpl, EventArgs.Empty);
		realEventCount.Should().Be(1);

		// Switch to mock and subscribe a new handler
		DefaultMockProvider<ServiceWithEvents>.SetMock(mock);
		EventHandler mockHandler = (s, e) => mockEventCount++;
		mock.StatusChanged += mockHandler;

		wrapper.StatusChanged += Raise.Event<EventHandler>(mock, EventArgs.Empty);
		mockEventCount.Should().Be(1);
		realEventCount.Should().Be(1); // Real handler not affected by mock events

		// Switch back to real
		DefaultMockProvider<ServiceWithEvents>.RemoveMock();
		wrapper.StatusChanged += Raise.Event<EventHandler>(realImpl, EventArgs.Empty);
		realEventCount.Should().Be(2); // Real handler still subscribed
	}

	[Fact]
	public void Should_HandleMultipleSubscribers()
	{
		var realImpl = Substitute.For<ServiceWithEvents>();
		var mock = Substitute.For<ServiceWithEvents>();
		var wrapper = new MockableServiceWithEventsDouble(realImpl);

		DefaultMockProvider<ServiceWithEvents>.SetMock(mock);

		var subscriber1Called = false;
		var subscriber2Called = false;
		var subscriber3Called = false;

		mock.StatusChanged += (s, e) => subscriber1Called = true;
		mock.StatusChanged += (s, e) => subscriber2Called = true;
		mock.StatusChanged += (s, e) => subscriber3Called = true;

		wrapper.StatusChanged += Raise.Event<EventHandler>(mock, EventArgs.Empty);

		subscriber1Called.Should().BeTrue();
		subscriber2Called.Should().BeTrue();
		subscriber3Called.Should().BeTrue();
	}

	[Fact]
	public void EventSubscriptions_AreTiedToActiveProviderAtSubscriptionTime()
	{
		// This test documents the behavior that subscriptions are bound
		// to whichever provider is active when the subscription occurs

		var realImpl = Substitute.For<ServiceWithEvents>();
		var mock = Substitute.For<ServiceWithEvents>();
		var wrapper = new MockableServiceWithEventsDouble(realImpl);

		var handler1Called = false;
		var handler2Called = false;

		// Subscribe with real provider active
		wrapper.StatusChanged += (s, e) => handler1Called = true;

		// Switch to mock
		DefaultMockProvider<ServiceWithEvents>.SetMock(mock);

		// Subscribe with mock provider active
		wrapper.StatusChanged += (s, e) => handler2Called = true;

		// Raise real event - only handler1 fires
		realImpl.StatusChanged += Raise.Event<EventHandler>(realImpl, EventArgs.Empty);
		handler1Called.Should().BeTrue();
		handler2Called.Should().BeFalse();

		handler1Called = false;

		// Raise mock event - only handler2 fires
		mock.StatusChanged += Raise.Event<EventHandler>(mock, EventArgs.Empty);
		handler1Called.Should().BeFalse();
		handler2Called.Should().BeTrue();
	}


	public class ConcreteClassWithEvents
	{
		public virtual event EventHandler? StatusChanged;
		public virtual event EventHandler<DataEventArgs>? DataReceived;
		public virtual event CustomEventHandler? CustomEvent;

		public void RaiseStatusChanged()
		{
			StatusChanged?.Invoke(this, EventArgs.Empty);
		}

		public void RaiseDataReceived(string data)
		{
			DataReceived?.Invoke(this, new DataEventArgs { Data = data });
		}

		public void RaiseCustomEvent(string message)
		{
			CustomEvent?.Invoke(this, message);
		}
	}

}