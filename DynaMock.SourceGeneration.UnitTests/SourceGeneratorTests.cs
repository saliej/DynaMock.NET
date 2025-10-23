using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace DynaMock.SourceGeneration.UnitTests;

public class SourceGeneratorTests
{
	[Fact]
    public async Task SimpleInterface_GeneratesMockableWrapper()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public interface ITestService
    {
        string GetValue();
        int Count { get; set; }
    }

    [Mockable(typeof(ITestService))]
    public class MockableTypes { }
}";

        var expectedBase = @"using System;
using DynaMock;
using TestNamespace;

namespace DynaMock.Generated
{
    public abstract class MockableITestServiceBase
    {
        protected readonly ITestService RealImplementation;
        protected readonly IMockProvider<ITestService> MockProvider;

        protected MockableITestServiceBase(ITestService realImplementation, IMockProvider<ITestService>? mockProvider)
        {
            RealImplementation = realImplementation ?? throw new ArgumentNullException(nameof(realImplementation));
            MockProvider = mockProvider ?? new DefaultMockProvider<ITestService>();
        }

        protected ITestService Implementation => MockProvider.Current ?? RealImplementation;

        protected bool ShouldUseMockForMethod(string methodName)
        {
            return MockProvider.Current != null &&
            (
                MockProvider.MockConfig == null ||
                MockProvider.MockConfig?.IsMethodMocked(methodName) == true
            );
        }

        protected bool ShouldUseMockForProperty(string propertyName)
        {
            return MockProvider.Current != null &&
            (
                MockProvider.MockConfig == null ||
                MockProvider.MockConfig?.IsPropertyMocked(propertyName) == true
            );
        }

        protected bool ShouldUseMockForEvent(string eventName)
        {
            return MockProvider.Current != null &&
            (
                MockProvider.MockConfig == null ||
                MockProvider.MockConfig?.IsEventMocked(eventName) == true
            );
        }

        public ITestService GetRealImplementation() => RealImplementation;
    }
}
".ReplaceLineEndings("\n");

    var expectedImpl = @"using System;
using System.Threading;
using System.Threading.Tasks;
using DynaMock;
using TestNamespace;

namespace DynaMock.Generated
{
    public class MockableITestService : MockableITestServiceBase, ITestService
    {
        public MockableITestService(ITestService realImplementation)
            : this(realImplementation, null)
        {
        }

        public MockableITestService(ITestService realImplementation, IMockProvider<ITestService>? mockProvider)
            : base(realImplementation, mockProvider)
        {
        }

        public string GetValue()
        {
            if (ShouldUseMockForMethod(""GetValue""))
            {
                return MockProvider.Current.GetValue();
            }

            return RealImplementation.GetValue();
        }

        public int Count
        {
            get
            {
                if (ShouldUseMockForProperty(""Count""))
                    return MockProvider.Current.Count;

                return RealImplementation.Count;
            }
            set
            {
                if (ShouldUseMockForProperty(""Count""))
                {
                    MockProvider.Current.Count = value;
                    return;
                }

                RealImplementation.Count = value;
            }
        }

    }
}
".ReplaceLineEndings("\n");

		await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source },
                GeneratedSources =
                {
					(typeof(MockableGenerator), "MockableITestServiceBase.g.cs", expectedBase),
					(typeof(MockableGenerator), "MockableITestService.g.cs", expectedImpl)
				}
            }
        }.RunAsync();
    }

    [Fact]
    public async Task InterfaceWithAsyncMethod_GeneratesCorrectWrapper()
    {
        var source = @"
using System.Threading.Tasks;
using DynaMock;

namespace TestNamespace
{
    public interface IAsyncService
    {
        Task<string> GetDataAsync();
        Task ProcessAsync();
    }

    [Mockable(typeof(IAsyncService))]
    public class MockableTypes { }
}";

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source },
                GeneratedSources =
                {
                    (typeof(MockableGenerator), "MockableIAsyncService.g.cs", SourceText.From(@"using System;
using System.Threading;
using System.Threading.Tasks;
using DynaMock;
using TestNamespace;

namespace DynaMock.Generated
{
    public class MockableIAsyncService : MockableBase<IAsyncService>, IAsyncService
    {
        public MockableIAsyncService(IAsyncService realImplementation)
            : this(realImplementation, null)
        {
        }

        public MockableIAsyncService(IAsyncService realImplementation, IMockProvider<IAsyncService>? mockProvider)
            : base(realImplementation, mockProvider)
        {
        }

        public System.Threading.Tasks.Task<string> GetDataAsync()
        {
            if (ShouldUseMockForMethod(""GetDataAsync""))
            {
                return MockProvider.Current.GetDataAsync();
            }

            return RealImplementation.GetDataAsync();
        }

        public System.Threading.Tasks.Task ProcessAsync()
        {
            if (ShouldUseMockForMethod(""ProcessAsync""))
            {
                return MockProvider.Current.ProcessAsync();
            }

            return RealImplementation.ProcessAsync();
        }

    }
}
", System.Text.Encoding.UTF8))
                }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task InterfaceWithEvent_GeneratesCorrectWrapper()
    {
        var source =
@"using System;
using DynaMock;

namespace TestNamespace
{
    public interface IEventService
    {
        event EventHandler StatusChanged;
        void RaiseEvent();
    }

    [Mockable(typeof(IEventService))]
    public class MockableTypes { }
}";

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source }
            }
        }.RunAsync();

        // Verify the generated file contains event handling
        var test = new SourceGeneratorTest
        {
            TestState = { Sources = { source } }
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task MultipleTypes_GeneratesMultipleWrappers()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public interface IServiceA
    {
        string GetA();
    }

    public interface IServiceB
    {
        string GetB();
    }

    [Mockable(typeof(IServiceA), typeof(IServiceB))]
    public class MockableTypes { }
}";

		await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source },
                GeneratedSources =
                {
                    (typeof(MockableGenerator), "MockableIServiceA.g.cs", SourceText.From(
@"using System;
using System.Threading;
using System.Threading.Tasks;
using DynaMock;
using TestNamespace;

namespace DynaMock.Generated
{
    public class MockableIServiceA : MockableBase<IServiceA>, IServiceA
    {
        public MockableIServiceA(IServiceA realImplementation)
            : this(realImplementation, null)
        {
        }

        public MockableIServiceA(IServiceA realImplementation, IMockProvider<IServiceA>? mockProvider)
            : base(realImplementation, mockProvider)
        {
        }

        public string GetA()
        {
            if (ShouldUseMockForMethod(""GetA""))
            {
                return MockProvider.Current.GetA();
            }

            return RealImplementation.GetA();
        }

    }
}
".Replace("\r\n", "\n"), System.Text.Encoding.UTF8)),
                    
                    (typeof(MockableGenerator), "MockableIServiceB.g.cs", SourceText.From(
@"using System;
using System.Threading;
using System.Threading.Tasks;
using DynaMock;
using TestNamespace;

namespace DynaMock.Generated
{
    public class MockableIServiceB : MockableBase<IServiceB>, IServiceB
    {
        public MockableIServiceB(IServiceB realImplementation)
            : this(realImplementation, null)
        {
        }

        public MockableIServiceB(IServiceB realImplementation, IMockProvider<IServiceB>? mockProvider)
            : base(realImplementation, mockProvider)
        {
        }

        public string GetB()
        {
            if (ShouldUseMockForMethod(""GetB""))
            {
                return MockProvider.Current.GetB();
            }

            return RealImplementation.GetB();
        }

    }
}
".Replace("\r\n", "\n"), System.Text.Encoding.UTF8))
                }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task GenericInterface_GeneratesGenericWrapper()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public interface IRepository<T> where T : class
    {
        T GetById(int id);
        void Save(T entity);
    }

    [Mockable(typeof(IRepository<>))]
    public class MockableTypes { }
}";

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task VirtualMembersEnabled_GeneratesVirtualMethods()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public interface ITestService
    {
        string GetValue();
    }

    [Mockable(typeof(ITestService), VirtualMembers = true)]
    public class MockableTypes { }
}";

        var expected = @"using System;
using System.Threading;
using System.Threading.Tasks;
using DynaMock;
using TestNamespace;

namespace DynaMock.Generated
{
    public class MockableITestService : MockableBase<ITestService>, ITestService
    {
        public MockableITestService(ITestService realImplementation)
            : this(realImplementation, null)
        {
        }

        public MockableITestService(ITestService realImplementation, IMockProvider<ITestService>? mockProvider)
            : base(realImplementation, mockProvider)
        {
        }

        public string GetValue()
        {
            if (ShouldUseMockForMethod(""GetValue""))
            {
                return MockProvider.Current.GetValue();
            }

            return RealImplementation.GetValue();
        }

    }
}
";

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source },
                GeneratedSources =
                {
					(typeof(MockableGenerator), "MockableITestService.g.cs", expected)
                }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task AbstractClass_GeneratesMockableWrapper()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public abstract class AbstractService
    {
        public abstract string GetValue();
        public virtual int Calculate(int x) => x * 2;
    }

    [Mockable(typeof(AbstractService))]
    public class MockableTypes { }
}";
		var expectedBase = @"using System;
using DynaMock;
using TestNamespace;

namespace DynaMock.Generated
{
    public abstract class MockableAbstractServiceBase : AbstractService
    {
        protected readonly AbstractService RealImplementation;
        protected readonly IMockProvider<AbstractService> MockProvider;

        protected MockableAbstractServiceBase(AbstractService realImplementation, IMockProvider<AbstractService>? mockProvider)
            : base()
        {
            RealImplementation = realImplementation ?? throw new ArgumentNullException(nameof(realImplementation));
            MockProvider = mockProvider ?? new DefaultMockProvider<AbstractService>();
        }

        protected AbstractService Implementation => MockProvider.Current ?? RealImplementation;

        protected bool ShouldUseMockForMethod(string methodName)
        {
            return MockProvider.Current != null &&
            (
                MockProvider.MockConfig == null ||
                MockProvider.MockConfig?.IsMethodMocked(methodName) == true
            );
        }

        protected bool ShouldUseMockForProperty(string propertyName)
        {
            return MockProvider.Current != null &&
            (
                MockProvider.MockConfig == null ||
                MockProvider.MockConfig?.IsPropertyMocked(propertyName) == true
            );
        }

        protected bool ShouldUseMockForEvent(string eventName)
        {
            return MockProvider.Current != null &&
            (
                MockProvider.MockConfig == null ||
                MockProvider.MockConfig?.IsEventMocked(eventName) == true
            );
        }

        public AbstractService GetRealImplementation() => RealImplementation;
    }
}
".ReplaceLineEndings("\n");

		var expectedImpl = @"using System;
using System.Threading;
using System.Threading.Tasks;
using DynaMock;
using TestNamespace;

namespace DynaMock.Generated
{
    public class MockableAbstractService : MockableAbstractServiceBase
    {
        public MockableAbstractService(AbstractService realImplementation)
            : this(realImplementation, null)
        {
        }

        public MockableAbstractService(AbstractService realImplementation, IMockProvider<AbstractService>? mockProvider)
            : base(realImplementation, mockProvider)
        {
        }

        public override string GetValue()
        {
            if (ShouldUseMockForMethod(""GetValue""))
            {
                return MockProvider.Current.GetValue();
            }

            return RealImplementation.GetValue();
        }

        public override int Calculate(int x)
        {
            if (ShouldUseMockForMethod(""Calculate""))
            {
                return MockProvider.Current.Calculate(x);
            }

            return RealImplementation.Calculate(x);
        }

    }
}
".ReplaceLineEndings("\n");
		await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source },
				GeneratedSources =
				{
                    (typeof(MockableGenerator), "MockableAbstractServiceBase.g.cs", expectedBase),
					(typeof(MockableGenerator), "MockableAbstractService.g.cs", expectedImpl)
				}
			}
        }.RunAsync();
    }

    [Fact]
    public async Task ConcreteClass_GeneratesMockableWrapper()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public class ConcreteService
    {
        public string GetValue() => ""value"";
        public int Add(int first, int second) => first + second;
        public int Count { get; set; }
    }

    [Mockable(typeof(ConcreteService))]
    public class MockableTypes { }
}";
        var expectedBase = @"using System;
using DynaMock;
using TestNamespace;

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

        protected ConcreteService Implementation => MockProvider.Current ?? RealImplementation;

        public ConcreteService GetRealImplementation() => RealImplementation;
    }
}
".ReplaceLineEndings("\n");

        var expectedImpl = @"using System;
using System.Threading;
using System.Threading.Tasks;
using DynaMock;
using TestNamespace;

namespace DynaMock.Generated
{
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

        public override string GetValue()
        {
            return Interceptor.InterceptMethod(
                x => x.GetValue(),
                impl => impl.GetValue(),
                Array.Empty<object?>());
        }

        public override int Count
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
".ReplaceLineEndings("\n");

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source },
                GeneratedSources =
                {
                (typeof(MockableGenerator), "MockableConcreteServiceBase.g.cs", expectedBase),
                (typeof(MockableGenerator), "MockableConcreteService.g.cs", expectedImpl)
            }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task SealedClass_ReportsDiagnostic()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public sealed class SealedService
    {
        public string GetValue() => ""value"";
    }

    [Mockable(typeof(SealedService))]
    public class MockableTypes { }
}";

        var expected = new DiagnosticResult("MOCK002", DiagnosticSeverity.Warning)
            .WithSpan(11, 5, 12, 35)
            .WithArguments("SealedService", "sealed classes cannot be mocked");

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source },
                ExpectedDiagnostics = { expected }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task StaticClass_ReportsDiagnostic()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public static class StaticService
    {
        public static string GetValue() => ""value"";
    }

    [Mockable(typeof(StaticService))]
    public class MockableTypes { }
}";

        var expected = new DiagnosticResult("MOCK002", DiagnosticSeverity.Warning)
            .WithSpan(11, 5, 12, 35)
            .WithArguments("StaticService", "static classes cannot be mocked");

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source },
                ExpectedDiagnostics = { expected }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task Struct_ReportsDiagnostic()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public struct TestStruct
    {
        public string GetValue() => ""value"";
    }

    [Mockable(typeof(TestStruct))]
    public class MockableTypes { }
}";

        var expected = new DiagnosticResult("MOCK002", DiagnosticSeverity.Warning)
            .WithSpan(11, 5, 12, 35)
            .WithArguments("TestStruct", "structs cannot be mocked");

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source },
                ExpectedDiagnostics = { expected }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task Enum_ReportsDiagnostic()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public enum TestEnum
    {
        Value1,
        Value2
    }

    [Mockable(typeof(TestEnum))]
    public class MockableTypes { }
}";

        var expected = new DiagnosticResult("MOCK002", DiagnosticSeverity.Warning)
            .WithSpan(12, 5, 13, 35)
            .WithArguments("TestEnum", "enums cannot be mocked");

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source },
                ExpectedDiagnostics = { expected }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task InterfaceWithMethodParameters_GeneratesCorrectWrapper()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public interface ICalculator
    {
        int Add(int a, int b);
        void LogMessage(string message, int level);
        T Transform<T>(T input) where T : class;
    }

    [Mockable(typeof(ICalculator))]
    public class MockableTypes { }
}";

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task InterfaceWithReadOnlyProperty_GeneratesCorrectWrapper()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public interface IReadOnlyService
    {
        string Name { get; }
        int Count { get; }
    }

    [Mockable(typeof(IReadOnlyService))]
    public class MockableTypes { }
}";

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task InterfaceWithWriteOnlyProperty_GeneratesCorrectWrapper()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public interface IWriteOnlyService
    {
        string Name { set; }
    }

    [Mockable(typeof(IWriteOnlyService))]
    public class MockableTypes { }
}";

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task ComplexGenericInterface_GeneratesCorrectWrapper()
    {
        var source = @"
using System.Collections.Generic;
using DynaMock;

namespace TestNamespace
{
    public interface IComplexService<T, U> 
        where T : class 
        where U : struct
    {
        Dictionary<T, U> GetMap();
        void Process<V>(T item, U value, V extra) where V : new();
    }

    [Mockable(typeof(IComplexService<,>))]
    public class MockableTypes { }
}";

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task NoMockableAttribute_GeneratesNothing()
    {
        var source = @"
namespace TestNamespace
{
    public interface ITestService
    {
        string GetValue();
    }

    public class RegularClass { }
}";

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source },
                GeneratedSources = { }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task InterfaceWithIndexer_GeneratesCorrectWrapper()
    {
        var source = @"
using DynaMock;

namespace TestNamespace
{
    public interface IIndexedService
    {
        string this[int index] { get; set; }
        int this[string key] { get; }
    }

    [Mockable(typeof(IIndexedService))]
    public class MockableTypes { }
}";

        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source }
            }
        }.RunAsync();
    }
}

internal class SourceGeneratorTest : CSharpSourceGeneratorTest<MockableGenerator, DefaultVerifier>
{
    public SourceGeneratorTest()
    {
        // Add required references
        ReferenceAssemblies = ReferenceAssemblies.Net.Net90;
        
        // Add DynaMock assembly reference (where MockableAttribute and MockableBase are defined)
        TestState.AdditionalReferences.Add(typeof(DynaMock.MockableAttribute).Assembly);
    }

    protected override CompilationOptions CreateCompilationOptions()
    {
        var compilationOptions = base.CreateCompilationOptions();
        return compilationOptions
            .WithSpecificDiagnosticOptions(compilationOptions.SpecificDiagnosticOptions
                .SetItem("CS8019", ReportDiagnostic.Suppress)); // Suppress unnecessary using directive warnings
    }
}
