//using System;
//using DynaMock.UnitTests.TestServices;

//namespace DynaMock.UnitTests.TestDoubles;

//public class MockableAbstractTestServiceDouble : MockableBase<AbstractTestService>
//{
//    public MockableAbstractTestServiceDouble(AbstractTestService realImplementation, 
//		IMockProvider<AbstractTestService>? mockProvider = null) : base(realImplementation, mockProvider)
//    {
//    }

//	public string Name
//	{
//		get
//		{
//			if (ShouldUseMockForProperty("Name"))
//				return MockProvider.Current.Name;

//			return RealImplementation.Name;
//		}
//		set
//		{
//			if (ShouldUseMockForProperty("Name"))
//				MockProvider.Current.Name = value;

//			RealImplementation.Name = value;
//		}
//	}

//	public string GetValue()
//    {
//        if (ShouldUseMockForMethod("GetValue"))
//            return MockProvider.Current.GetValue();

//        return RealImplementation.GetValue();
//    }

//    public Task<int> GetCountAsync()
//    {
//        if (ShouldUseMockForMethod("GetCountAsync"))
//            return MockProvider.Current.GetCountAsync();

//        return RealImplementation.GetCountAsync();
//    }

//    public string GetDefaultValue()
//    {
//        if (ShouldUseMockForMethod("GetDefaultValue"))
//            return MockProvider.Current.GetDefaultValue();

//        return RealImplementation.GetDefaultValue();
//    }
//}