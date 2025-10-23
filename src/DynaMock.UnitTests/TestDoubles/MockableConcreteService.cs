using System;
using DynaMock;
using DynaMock.UnitTests.TestServices;

namespace DynaMock.Generated
{
    public abstract class MockableConcreteServiceBase : ConcreteService
    {
        protected readonly ConcreteService RealImplementation;
        protected readonly IMockProvider<ConcreteService> MockProvider;
        protected readonly CallInterceptor<ConcreteService> Interceptor;

        protected MockableConcreteServiceBase(ConcreteService realImplementation, IMockProvider<ConcreteService>? mockProvider)
            : base()
        {
            RealImplementation = realImplementation ?? throw new ArgumentNullException(nameof(realImplementation));
            MockProvider = mockProvider ?? new DefaultMockProvider<ConcreteService>();
            Interceptor = new CallInterceptor<ConcreteService>(MockProvider, RealImplementation);
        }

        public ConcreteService GetRealImplementation() => RealImplementation;
    }

    public class MockableConcreteService : MockableConcreteServiceBase
    {
        public MockableConcreteService(ConcreteService realImplementation)
            : this(realImplementation, null)
        {
        }

        public MockableConcreteService(ConcreteService realImplementation, IMockProvider<ConcreteService>? mockProvider)
            : base(realImplementation, mockProvider)
        {
        }

        public string GetValue()
        {
            return Interceptor.InterceptMethod(
                x => x.GetValue(),
                impl => impl.GetValue(),
                Array.Empty<object?>());
        }

        public int Add(int first, int second)
        {
            return Interceptor.InterceptMethod(
                x => x.Add(first, second),
                impl => impl.Add(first, second),
                new object?[] { first, second });
        }

        public int Count
        {
            get
            {
                return Interceptor.InterceptPropertyGet(
                    x => x.Count,
                    impl => impl.Count);
            }
            set
            {
                Interceptor.InterceptPropertySet(
                    x => x.Count,
                    impl => impl.Count = value,
                    value);
            }
        }
    }
}
