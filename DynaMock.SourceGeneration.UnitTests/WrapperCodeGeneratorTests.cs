using System;
using AwesomeAssertions;
using DynaMock.SourceGeneration.Models;

namespace DynaMock.SourceGeneration.UnitTests;

public class WrapperCodeGeneratorTests
{
    private readonly WrapperCodeGenerator _sut;

    public WrapperCodeGeneratorTests()
    {
        _sut = new WrapperCodeGenerator();
    }

    [Fact]
    public void GenerateWrapperClass_SimpleClass_GeneratesCorrectStructure()
    {
        var model = new TypeModel
        {
            Name = "TestClass",
            Namespace = "TestNamespace",
            IsInterface = false,
            GenericTypeParameters = new List<string>(),
            GenericTypeConstraints = new List<string>(),
            Methods = new List<MethodModel>(),
            Properties = new List<PropertyModel>(),
            Events = new List<EventModel>()
        };

        var result = _sut.GenerateWrapperClass(model, virtualMembers: false);

        result.Should().Contain("using System;");
        result.Should().Contain("using System.Threading;");
        result.Should().Contain("using System.Threading.Tasks;");
        result.Should().Contain("using System.Linq.Expressions;");
        result.Should().Contain("using DynaMock;");
        result.Should().Contain("using TestNamespace;");
        result.Should().Contain("namespace DynaMock.Generated");
        result.Should().Contain("public class MockableTestClass : MockableTestClassBase");
        result.Should().Contain("public MockableTestClass(TestClass realImplementation)");
        result.Should().Contain("public MockableTestClass(TestClass realImplementation, IMockProvider<TestClass>? mockProvider)");
    }

    [Fact]
    public void GenerateWrapperClass_ClassWithGenericParameters_GeneratesGenericClass()
    {
        var model = new TypeModel
        {
            Name = "Repository",
            Namespace = "Data",
            IsInterface = false,
            GenericTypeParameters = new List<string> { "T", "TKey" },
            GenericTypeConstraints = new List<string> { "where T : class", "where TKey : struct" },
            Methods = new List<MethodModel>(),
            Properties = new List<PropertyModel>(),
            Events = new List<EventModel>()
        };

        var result = _sut.GenerateWrapperClass(model, virtualMembers: false);

        result.Should().Contain("public class MockableRepository<T, TKey> : MockableRepositoryBase<T, TKey>");
        result.Should().Contain("where T : class");
        result.Should().Contain("where TKey : struct");
        result.Should().Contain("public MockableRepository(Repository<T, TKey> realImplementation)");
        result.Should().Contain("public MockableRepository(Repository<T, TKey> realImplementation, IMockProvider<Repository<T, TKey>>? mockProvider)");
    }

    [Fact]
    public void GenerateWrapperClass_Interface_ImplementsInterface()
    {
        var model = new TypeModel
        {
            Name = "IService",
            Namespace = "Services",
            IsInterface = true,
            GenericTypeParameters = new List<string>(),
            GenericTypeConstraints = new List<string>(),
            Methods = new List<MethodModel>(),
            Properties = new List<PropertyModel>(),
            Events = new List<EventModel>()
        };

        var result = _sut.GenerateWrapperClass(model, virtualMembers: false);

        result.Should().Contain("public class MockableIService : MockableIServiceBase, IService");
    }

    [Fact]
    public void GenerateWrapperClass_InterfaceWithGenericParameters_ImplementsGenericInterface()
    {
        var model = new TypeModel
        {
            Name = "IRepository",
            Namespace = "Data",
            IsInterface = true,
            GenericTypeParameters = new List<string> { "T" },
            GenericTypeConstraints = new List<string> { "where T : class" },
            Methods = new List<MethodModel>(),
            Properties = new List<PropertyModel>(),
            Events = new List<EventModel>()
        };

        var result = _sut.GenerateWrapperClass(model, virtualMembers: false);

        result.Should().Contain("public class MockableIRepository<T> : MockableIRepositoryBase<T>, IRepository<T>");
        result.Should().Contain("where T : class");
    }

    [Fact]
    public void GenerateWrapperClass_NamespaceIsDynaMockGenerated_DoesNotIncludeUsingForNamespace()
    {
        var model = new TypeModel
        {
            Name = "TestClass",
            Namespace = "DynaMock.Generated",
            IsInterface = false,
            GenericTypeParameters = new List<string>(),
            GenericTypeConstraints = new List<string>(),
            Methods = new List<MethodModel>(),
            Properties = new List<PropertyModel>(),
            Events = new List<EventModel>()
        };

        var result = _sut.GenerateWrapperClass(model, virtualMembers: false);

        result.Should().NotContain("using DynaMock.Generated;");
    }

    [Fact]
    public void GenerateWrapperClass_EmptyNamespace_DoesNotIncludeEmptyUsing()
    {
        var model = new TypeModel
        {
            Name = "TestClass",
            Namespace = "",
            IsInterface = false,
            GenericTypeParameters = new List<string>(),
            GenericTypeConstraints = new List<string>(),
            Methods = new List<MethodModel>(),
            Properties = new List<PropertyModel>(),
            Events = new List<EventModel>()
        };

        var result = _sut.GenerateWrapperClass(model, virtualMembers: false);

        result.Should().NotContain("using ;");
    }

    [Fact]
    public void GenerateWrapperClass_NullNamespace_DoesNotIncludeNullUsing()
    {
        var model = new TypeModel
        {
            Name = "TestClass",
            Namespace = null,
            IsInterface = false,
            GenericTypeParameters = new List<string>(),
            GenericTypeConstraints = new List<string>(),
            Methods = new List<MethodModel>(),
            Properties = new List<PropertyModel>(),
            Events = new List<EventModel>()
        };

        var result = _sut.GenerateWrapperClass(model, virtualMembers: false);

        result.Should().NotContain("using ;");
    }

    [Fact]
    public void GenerateWrapperClass_NormalizesLineEndingsToLF()
    {
        var model = new TypeModel
        {
            Name = "TestClass",
            Namespace = "Test",
            IsInterface = false,
            GenericTypeParameters = new List<string>(),
            GenericTypeConstraints = new List<string>(),
            Methods = new List<MethodModel>(),
            Properties = new List<PropertyModel>(),
            Events = new List<EventModel>()
        };

        var result = _sut.GenerateWrapperClass(model, virtualMembers: false);

        result.Should().NotContain("\r\n");
        result.Should().Contain("\n");
    }

    [Fact]
    public void GenerateWrapperClass_GeneratesAllRequiredUsings()
    {
        var model = new TypeModel
        {
            Name = "TestClass",
            Namespace = "Test",
            IsInterface = false,
            GenericTypeParameters = new List<string>(),
            GenericTypeConstraints = new List<string>(),
            Methods = new List<MethodModel>(),
            Properties = new List<PropertyModel>(),
            Events = new List<EventModel>()
        };

        var result = _sut.GenerateWrapperClass(model, virtualMembers: false);

        result.Should().Contain("using System;");
        result.Should().Contain("using System.Threading;");
        result.Should().Contain("using System.Threading.Tasks;");
        result.Should().Contain("using System.Linq.Expressions;");
        result.Should().Contain("using DynaMock;");
    }

    [Fact]
    public void GenerateWrapperClass_GeneratesBothConstructors()
    {
        var model = new TypeModel
        {
            Name = "Service",
            Namespace = "Services",
            IsInterface = false,
            GenericTypeParameters = new List<string>(),
            GenericTypeConstraints = new List<string>(),
            Methods = new List<MethodModel>(),
            Properties = new List<PropertyModel>(),
            Events = new List<EventModel>()
        };

        var result = _sut.GenerateWrapperClass(model, virtualMembers: false);

        result.Should().Contain("public MockableService(Service realImplementation)");
        result.Should().Contain(": this(realImplementation, null)");
        result.Should().Contain("public MockableService(Service realImplementation, IMockProvider<Service>? mockProvider)");
        result.Should().Contain(": base(realImplementation, mockProvider)");
    }
}