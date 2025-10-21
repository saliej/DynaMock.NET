using DynaMock;
using DynaMock.UnitTests;

/// <summary>
/// Test double simulating generated mockable wrapper
/// </summary>
public class MockableServiceWithEventsDouble : MockableBase<ServiceWithEvents>
{
	public MockableServiceWithEventsDouble(ServiceWithEvents realImplementation)
		: this(realImplementation, null)
	{
	}

	public MockableServiceWithEventsDouble(
		ServiceWithEvents realImplementation,
		IMockProvider<ServiceWithEvents>? mockProvider)
		: base(realImplementation, mockProvider)
	{
	}

	public virtual event EventHandler? StatusChanged
	{
		add
		{
			if (ShouldUseMockForEvent(nameof(StatusChanged)))
			{
				MockProvider.Current.StatusChanged += value;
				return;
			}
			RealImplementation.StatusChanged += value;
		}
		remove
		{
			if (ShouldUseMockForEvent(nameof(StatusChanged)))
			{
				MockProvider.Current.StatusChanged -= value;
				return;
			}
			RealImplementation.StatusChanged -= value;
		}
	}

	public virtual event EventHandler<DataEventArgs>? DataReceived
	{
		add
		{
			if (ShouldUseMockForEvent(nameof(DataReceived)))
			{
				MockProvider.Current.DataReceived += value;
				return;
			}
			RealImplementation.DataReceived += value;
		}
		remove
		{
			if (ShouldUseMockForEvent(nameof(DataReceived)))
			{
				MockProvider.Current.DataReceived -= value;
				return;
			}
			RealImplementation.DataReceived -= value;
		}
	}

	public virtual event CustomEventHandler? CustomEvent
	{
		add
		{
			if (ShouldUseMockForEvent(nameof(CustomEvent)))
			{
				MockProvider.Current.CustomEvent += value;
				return;
			}
			RealImplementation.CustomEvent += value;
		}
		remove
		{
			if (ShouldUseMockForEvent(nameof(CustomEvent)))
			{
				MockProvider.Current.CustomEvent -= value;
				return;
			}
			RealImplementation.CustomEvent -= value;
		}
	}
}