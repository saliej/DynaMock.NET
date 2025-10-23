//using DynaMock;
//using DynaMock.UnitTests;
//using DynaMock.UnitTests.TestServices;

//namespace DynaMock.UnitTests.TestDoubles;

///// <summary>
///// Test double simulating MockableVirtualTestService with VirtualMembers = true
///// All members are virtual to allow further mocking
///// </summary>
//public class MockableVirtualTestServiceDouble : MockableBase<VirtualTestService>
//{
//	// Single-parameter constructor (for ForPartsOf compatibility)
//	public MockableVirtualTestServiceDouble(VirtualTestService realImplementation)
//		: this(realImplementation, null)
//	{
//	}

//	// Two-parameter constructor (for explicit provider injection)
//	public MockableVirtualTestServiceDouble(
//		VirtualTestService realImplementation,
//		IMockProvider<VirtualTestService>? mockProvider)
//		: base(realImplementation, mockProvider)
//	{
//	}

//	// Note: All members are VIRTUAL (simulating VirtualMembers = true)
//	public virtual string Name
//	{
//		get
//		{
//			if (ShouldUseMockForProperty(nameof(Name)))
//				return MockProvider.Current.Name;
//			return RealImplementation.Name;
//		}
//		set
//		{
//			if (ShouldUseMockForProperty(nameof(Name)))
//			{
//				MockProvider.Current.Name = value;
//				return;
//			}
//			RealImplementation.Name = value;
//		}
//	}

//	public virtual int Count
//	{
//		get
//		{
//			if (ShouldUseMockForProperty(nameof(Count)))
//				return MockProvider.Current.Count;
//			return RealImplementation.Count;
//		}
//		set
//		{
//			if (ShouldUseMockForProperty(nameof(Count)))
//			{
//				MockProvider.Current.Count = value;
//				return;
//			}
//			RealImplementation.Count = value;
//		}
//	}

//	public virtual string GetValue()
//	{
//		if (ShouldUseMockForMethod(nameof(GetValue)))
//			return MockProvider.Current.GetValue();
//		return RealImplementation.GetValue();
//	}

//	public virtual string GetDefaultValue()
//	{
//		if (ShouldUseMockForMethod(nameof(GetDefaultValue)))
//			return MockProvider.Current.GetDefaultValue();
//		return RealImplementation.GetDefaultValue();
//	}

//	public virtual int Calculate(int a, int b)
//	{
//		if (ShouldUseMockForMethod(nameof(Calculate)))
//			return MockProvider.Current.Calculate(a, b);
//		return RealImplementation.Calculate(a, b);
//	}

//	public virtual void DoSomething()
//	{
//		if (ShouldUseMockForMethod(nameof(DoSomething)))
//		{
//			MockProvider.Current.DoSomething();
//			return;
//		}
//		RealImplementation.DoSomething();
//	}
//}