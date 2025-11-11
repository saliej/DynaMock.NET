using AwesomeAssertions;
using DynaMock.SourceGeneration.Models;
using System.Text;

namespace DynaMock.SourceGeneration.UnitTests;

public class CodeGeneratorHelperTests
{
    private readonly CodeGeneratorHelper _sut = new();

    [Fact]
    public void GenerateMethod_VoidMethodWithNoParameters_GeneratesCorrectCode()
    {
        var builder = new StringBuilder();
        var method = new MethodModel
        {
            Name = "DoSomething",
            ReturnType = "void",
            Parameters = new List<(string Type, string Name)>(),
            TypeParameters = new List<string>(),
            Constraints = new List<string>(),
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateMethod(builder, method, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public new virtual void DoSomething()");
        result.Should().Contain("Interceptor.InterceptVoidMethod(");
        result.Should().Contain("x => x.DoSomething(),");
        result.Should().Contain("impl => impl.DoSomething(),");
        result.Should().Contain("Array.Empty<object?>());");
    }

    [Fact]
    public void GenerateMethod_MethodWithReturnType_GeneratesCorrectCode()
    {
        var builder = new StringBuilder();
        var method = new MethodModel
        {
            Name = "GetValue",
            ReturnType = "int",
            Parameters = new List<(string Type, string Name)>(),
            TypeParameters = new List<string>(),
            Constraints = new List<string>(),
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateMethod(builder, method, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public new virtual int GetValue()");
        result.Should().Contain("return Interceptor.InterceptMethod(");
        result.Should().Contain("x => x.GetValue(),");
        result.Should().Contain("impl => impl.GetValue(),");
        result.Should().Contain("Array.Empty<object?>());");
    }

    [Fact]
    public void GenerateMethod_MethodWithParameters_GeneratesCorrectCode()
    {
        var builder = new StringBuilder();
        var method = new MethodModel
        {
            Name = "Calculate",
            ReturnType = "decimal",
            Parameters = new List<(string Type, string Name)>
            {
                ("int", "x"),
                ("double", "y")
            },
            TypeParameters = new List<string>(),
            Constraints = new List<string>(),
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateMethod(builder, method, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public new virtual decimal Calculate(int x, double y)");
        result.Should().Contain("x => x.Calculate(x, y),");
        result.Should().Contain("impl => impl.Calculate(x, y),");
        result.Should().Contain("new object?[] { x, y });");
    }

    [Fact]
    public void GenerateMethod_GenericMethod_GeneratesCorrectCode()
    {
        var builder = new StringBuilder();
        var method = new MethodModel
        {
            Name = "Process",
            ReturnType = "T",
            Parameters = new List<(string Type, string Name)>
            {
                ("T", "value")
            },
            TypeParameters = new List<string> { "T" },
            Constraints = new List<string> { "where T : class" },
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateMethod(builder, method, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public new virtual T Process<T>(T value)");
        result.Should().Contain("where T : class");
        result.Should().Contain("x => x.Process<T>(value),");
        result.Should().Contain("impl => impl.Process<T>(value),");
        result.Should().Contain("new object?[] { value });");
    }

    [Fact]
    public void GenerateMethod_VirtualMethod_GeneratesOverrideModifier()
    {
        var builder = new StringBuilder();
        var method = new MethodModel
        {
            Name = "Execute",
            ReturnType = "void",
            Parameters = new List<(string Type, string Name)>(),
            TypeParameters = new List<string>(),
            Constraints = new List<string>(),
            IsVirtual = true,
            IsAbstract = false
        };

        _sut.GenerateMethod(builder, method, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public override void Execute()");
    }

    [Fact]
    public void GenerateMethod_AbstractMethod_GeneratesOverrideModifier()
    {
        var builder = new StringBuilder();
        var method = new MethodModel
        {
            Name = "Process",
            ReturnType = "void",
            Parameters = new List<(string Type, string Name)>(),
            TypeParameters = new List<string>(),
            Constraints = new List<string>(),
            IsVirtual = false,
            IsAbstract = true
        };

        _sut.GenerateMethod(builder, method, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public override void Process()");
    }

    [Fact]
    public void GenerateMethod_InterfaceMethod_GeneratesPublicModifierOnly()
    {
        var builder = new StringBuilder();
        var method = new MethodModel
        {
            Name = "DoWork",
            ReturnType = "void",
            Parameters = new List<(string Type, string Name)>(),
            TypeParameters = new List<string>(),
            Constraints = new List<string>(),
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateMethod(builder, method, isInterface: true, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public void DoWork()");
        result.Should().NotContain("override");
        result.Should().NotContain("virtual");
    }

    [Fact]
    public void GenerateProperty_PropertyWithGetterOnly_GeneratesCorrectCode()
    {
        var builder = new StringBuilder();
        var property = new PropertyModel
        {
            Name = "Value",
            Type = "int",
            HasGetter = true,
            HasSetter = false,
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateProperty(builder, property, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public int Value");
        result.Should().Contain("get");
        result.Should().Contain("return Interceptor.InterceptPropertyGet(");
        result.Should().Contain("x => x.Value,");
        result.Should().Contain("impl => impl.Value);");
        result.Should().NotContain("set");
    }

    [Fact]
    public void GenerateProperty_PropertyWithSetterOnly_GeneratesCorrectCode()
    {
        var builder = new StringBuilder();
        var property = new PropertyModel
        {
            Name = "Value",
            Type = "string",
            HasGetter = false,
            HasSetter = true,
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateProperty(builder, property, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public string Value");
        result.Should().Contain("set");
        result.Should().Contain("Interceptor.InterceptPropertySet(");
        result.Should().Contain("x => x.Value,");
        result.Should().Contain("impl => impl.Value = value,");
        result.Should().Contain("value);");
        result.Should().NotContain("get");
    }

    [Fact]
    public void GenerateProperty_PropertyWithGetterAndSetter_GeneratesCorrectCode()
    {
        var builder = new StringBuilder();
        var property = new PropertyModel
        {
            Name = "Name",
            Type = "string",
            HasGetter = true,
            HasSetter = true,
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateProperty(builder, property, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public string Name");
        result.Should().Contain("get");
        result.Should().Contain("return Interceptor.InterceptPropertyGet(");
        result.Should().Contain("set");
        result.Should().Contain("Interceptor.InterceptPropertySet(");
    }

    [Fact]
    public void GenerateProperty_VirtualProperty_GeneratesOverrideModifier()
    {
        var builder = new StringBuilder();
        var property = new PropertyModel
        {
            Name = "Count",
            Type = "int",
            HasGetter = true,
            HasSetter = false,
            IsVirtual = true,
            IsAbstract = false
        };

        _sut.GenerateProperty(builder, property, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public override int Count");
    }

    [Fact]
    public void GenerateProperty_AbstractProperty_GeneratesOverrideModifier()
    {
        var builder = new StringBuilder();
        var property = new PropertyModel
        {
            Name = "Status",
            Type = "string",
            HasGetter = true,
            HasSetter = false,
            IsVirtual = false,
            IsAbstract = true
        };

        _sut.GenerateProperty(builder, property, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public override string Status");
    }

    [Fact]
    public void GenerateProperty_NonVirtualPropertyWithVirtualMembersFlag_GeneratesVirtualModifier()
    {
        var builder = new StringBuilder();
        var property = new PropertyModel
        {
            Name = "Data",
            Type = "object",
            HasGetter = true,
            HasSetter = false,
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateProperty(builder, property, isInterface: false, virtualMembers: true);

        var result = builder.ToString();
        result.Should().Contain("public virtual object Data");
    }

    [Fact]
    public void GenerateProperty_InterfaceProperty_GeneratesPublicModifierOnly()
    {
        var builder = new StringBuilder();
        var property = new PropertyModel
        {
            Name = "Value",
            Type = "int",
            HasGetter = true,
            HasSetter = true,
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateProperty(builder, property, isInterface: true, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public int Value");
        result.Should().NotContain("override");
        result.Should().NotContain("virtual");
    }

    [Fact]
    public void GenerateEvent_GeneratesCorrectCode()
    {
        var builder = new StringBuilder();
        var evt = new EventModel
        {
            Name = "Changed",
            Type = "EventHandler",
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateEvent(builder, evt, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public event EventHandler Changed");
        result.Should().Contain("add");
        result.Should().Contain("Interceptor.InterceptEventAdd(");
        result.Should().Contain("\"Changed\",");
        result.Should().Contain("impl => impl.Changed += value);");
        result.Should().Contain("remove");
        result.Should().Contain("Interceptor.InterceptEventRemove(");
        result.Should().Contain("impl => impl.Changed -= value);");
    }

    [Fact]
    public void GenerateEvent_VirtualEvent_GeneratesOverrideModifier()
    {
        var builder = new StringBuilder();
        var evt = new EventModel
        {
            Name = "Updated",
            Type = "EventHandler<int>",
            IsVirtual = true,
            IsAbstract = false
        };

        _sut.GenerateEvent(builder, evt, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public override event EventHandler<int> Updated");
    }

    [Fact]
    public void GenerateEvent_AbstractEvent_GeneratesOverrideModifier()
    {
        var builder = new StringBuilder();
        var evt = new EventModel
        {
            Name = "Completed",
            Type = "Action",
            IsVirtual = false,
            IsAbstract = true
        };

        _sut.GenerateEvent(builder, evt, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public override event Action Completed");
    }

    [Fact]
    public void GenerateEvent_NonVirtualEventWithVirtualMembersFlag_GeneratesVirtualModifier()
    {
        var builder = new StringBuilder();
        var evt = new EventModel
        {
            Name = "StateChanged",
            Type = "EventHandler",
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateEvent(builder, evt, isInterface: false, virtualMembers: true);

        var result = builder.ToString();
        result.Should().Contain("public virtual event EventHandler StateChanged");
    }

    [Fact]
    public void GenerateEvent_InterfaceEvent_GeneratesPublicModifierOnly()
    {
        var builder = new StringBuilder();
        var evt = new EventModel
        {
            Name = "Fired",
            Type = "EventHandler",
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateEvent(builder, evt, isInterface: true, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public event EventHandler Fired");
        result.Should().NotContain("override");
        result.Should().NotContain("virtual");
    }

    [Fact]
    public void GenerateMethod_MultipleTypeParameters_GeneratesCorrectCode()
    {
        var builder = new StringBuilder();
        var method = new MethodModel
        {
            Name = "Transform",
            ReturnType = "TResult",
            Parameters = new List<(string Type, string Name)>
            {
                ("TSource", "source")
            },
            TypeParameters = new List<string> { "TSource", "TResult" },
            Constraints = new List<string> { "where TSource : class", "where TResult : struct" },
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateMethod(builder, method, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public new virtual TResult Transform<TSource, TResult>(TSource source)");
        result.Should().Contain("where TSource : class");
        result.Should().Contain("where TResult : struct");
        result.Should().Contain("x => x.Transform<TSource, TResult>(source),");
    }

    [Fact]
    public void GenerateMethod_ComplexParameterTypes_GeneratesCorrectCode()
    {
        var builder = new StringBuilder();
        var method = new MethodModel
        {
            Name = "Process",
            ReturnType = "Task<List<string>>",
            Parameters = new List<(string Type, string Name)>
            {
                ("IEnumerable<int>", "items"),
                ("CancellationToken", "cancellationToken")
            },
            TypeParameters = new List<string>(),
            Constraints = new List<string>(),
            IsVirtual = false,
            IsAbstract = false
        };

        _sut.GenerateMethod(builder, method, isInterface: false, virtualMembers: false);

        var result = builder.ToString();
        result.Should().Contain("public new virtual Task<List<string>> Process(IEnumerable<int> items, CancellationToken cancellationToken)");
        result.Should().Contain("new object?[] { items, cancellationToken });");
    }
}