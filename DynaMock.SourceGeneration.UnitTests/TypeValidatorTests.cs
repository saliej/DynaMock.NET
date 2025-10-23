using System;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DynaMock.SourceGeneration.Tests;

public class TypeValidatorTests
{
	private readonly TypeValidator _validator;

	public TypeValidatorTests()
	{
		_validator = new TypeValidator();
	}

	#region Valid Mockable Types

	[Fact]
	public void CanBeMocked_Interface_ShouldReturnTrue()
	{
		var typeSymbol = GetTypeSymbol(@"
                public interface ITestInterface
                {
                    void DoSomething();
                }
            ", "ITestInterface");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	[Fact]
	public void CanBeMocked_AbstractClass_ShouldReturnTrue()
	{
		var typeSymbol = GetTypeSymbol(@"
                public abstract class AbstractTestClass
                {
                    public abstract void DoSomething();
                }
            ", "AbstractTestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	[Fact]
	public void CanBeMocked_AbstractClassWithoutParameterlessConstructor_ShouldReturnTrue()
	{
		var typeSymbol = GetTypeSymbol(@"
                public abstract class AbstractTestClass
                {
                    public AbstractTestClass(string name) { }
                    public abstract void DoSomething();
                }
            ", "AbstractTestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	[Fact]
	public void CanBeMocked_NonSealedClassWithParameterlessConstructor_ShouldReturnTrue()
	{
		var typeSymbol = GetTypeSymbol(@"
                public class TestClass
                {
                    public TestClass() { }
                    public virtual void DoSomething() { }
                }
            ", "TestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	[Fact]
	public void CanBeMocked_NonSealedClassWithImplicitParameterlessConstructor_ShouldReturnTrue()
	{
		var typeSymbol = GetTypeSymbol(@"
                public class TestClass
                {
                    public virtual void DoSomething() { }
                }
            ", "TestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	[Fact]
	public void CanBeMocked_NonSealedClassWithMultipleConstructors_IncludingParameterless_ShouldReturnTrue()
	{
		var typeSymbol = GetTypeSymbol(@"
                public class TestClass
                {
                    public TestClass() { }
                    public TestClass(string name) { }
                    public TestClass(int id, string name) { }
                    public virtual void DoSomething() { }
                }
            ", "TestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	[Fact]
	public void CanBeMocked_ObjectType_ShouldReturnTrue()
	{
		var compilation = CreateCompilation("");
		var typeSymbol = compilation.GetSpecialType(SpecialType.System_Object);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	#endregion

	#region Sealed Classes

	[Fact]
	public void CanBeMocked_SealedClass_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public sealed class SealedTestClass
                {
                    public void DoSomething() { }
                }
            ", "SealedTestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("sealed classes cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_SealedClassWithParameterlessConstructor_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public sealed class SealedTestClass
                {
                    public SealedTestClass() { }
                    public void DoSomething() { }
                }
            ", "SealedTestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("sealed classes cannot be mocked");
	}

	#endregion

	#region Static Classes

	[Fact]
	public void CanBeMocked_StaticClass_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public static class StaticTestClass
                {
                    public static void DoSomething() { }
                }
            ", "StaticTestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("static classes cannot be mocked");
	}

	#endregion

	#region Primitive Types

	[Fact]
	public void CanBeMocked_Int_ShouldReturnFalse()
	{
		var compilation = CreateCompilation("");
		var typeSymbol = compilation.GetSpecialType(SpecialType.System_Int32);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("primitive types cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_String_ShouldReturnFalse()
	{
		var compilation = CreateCompilation("");
		var typeSymbol = compilation.GetSpecialType(SpecialType.System_String);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("primitive types cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_Bool_ShouldReturnFalse()
	{
		var compilation = CreateCompilation("");
		var typeSymbol = compilation.GetSpecialType(SpecialType.System_Boolean);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("primitive types cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_Double_ShouldReturnFalse()
	{
		var compilation = CreateCompilation("");
		var typeSymbol = compilation.GetSpecialType(SpecialType.System_Double);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("primitive types cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_Decimal_ShouldReturnFalse()
	{
		var compilation = CreateCompilation("");
		var typeSymbol = compilation.GetSpecialType(SpecialType.System_Decimal);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("primitive types cannot be mocked");
	}

	#endregion

	#region Delegates

	[Fact]
	public void CanBeMocked_Delegate_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public delegate void TestDelegate(string message);
            ", "TestDelegate");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("delegates cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_FuncDelegate_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public delegate int TestFunc(string message);
            ", "TestFunc");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("delegates cannot be mocked");
	}

	#endregion

	#region Enums

	[Fact]
	public void CanBeMocked_Enum_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public enum TestEnum
                {
                    Value1,
                    Value2,
                    Value3
                }
            ", "TestEnum");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("enums cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_EnumWithExplicitValues_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public enum TestEnum
                {
                    Value1 = 1,
                    Value2 = 2,
                    Value3 = 4
                }
            ", "TestEnum");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("enums cannot be mocked");
	}

	#endregion

	#region Structs

	[Fact]
	public void CanBeMocked_Struct_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public struct TestStruct
                {
                    public int Value { get; set; }
                }
            ", "TestStruct");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("structs cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_StructWithConstructor_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public struct TestStruct
                {
                    public TestStruct(int value)
                    {
                        Value = value;
                    }
                    public int Value { get; set; }
                }
            ", "TestStruct");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("structs cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_ReadonlyStruct_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public readonly struct TestStruct
                {
                    public int Value { get; }
                }
            ", "TestStruct");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("structs cannot be mocked");
	}

	#endregion

	#region Constructor Requirements

	[Fact]
	public void CanBeMocked_ConcreteClassWithoutParameterlessConstructor_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public class TestClass
                {
                    public TestClass(string name) { }
                    public virtual void DoSomething() { }
                }
            ", "TestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("concrete classes must have a public parameterless constructor");
	}

	[Fact]
	public void CanBeMocked_ConcreteClassWithOnlyParameterizedConstructors_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public class TestClass
                {
                    public TestClass(string name) { }
                    public TestClass(int id, string name) { }
                    public virtual void DoSomething() { }
                }
            ", "TestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("concrete classes must have a public parameterless constructor");
	}

	[Fact]
	public void CanBeMocked_ConcreteClassWithPrivateParameterlessConstructor_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public class TestClass
                {
                    private TestClass() { }
                    public virtual void DoSomething() { }
                }
            ", "TestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("concrete classes must have a public parameterless constructor");
	}

	[Fact]
	public void CanBeMocked_ConcreteClassWithProtectedParameterlessConstructor_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public class TestClass
                {
                    protected TestClass() { }
                    public virtual void DoSomething() { }
                }
            ", "TestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("concrete classes must have a public parameterless constructor");
	}

	[Fact]
	public void CanBeMocked_ConcreteClassWithInternalParameterlessConstructor_ShouldReturnFalse()
	{
		var typeSymbol = GetTypeSymbol(@"
                public class TestClass
                {
                    internal TestClass() { }
                    public virtual void DoSomething() { }
                }
            ", "TestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("concrete classes must have a public parameterless constructor");
	}

	#endregion

	#region Edge Cases

	[Fact]
	public void CanBeMocked_NestedPublicInterface_ShouldReturnTrue()
	{
		var typeSymbol = GetTypeSymbol(@"
                public class OuterClass
                {
                    public interface INestedInterface
                    {
                        void DoSomething();
                    }
                }
            ", "INestedInterface");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	[Fact]
	public void CanBeMocked_GenericInterface_ShouldReturnTrue()
	{
		var typeSymbol = GetTypeSymbol(@"
                public interface ITestInterface<T>
                {
                    void DoSomething(T value);
                }
            ", "ITestInterface");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	[Fact]
	public void CanBeMocked_GenericClass_ShouldReturnTrue()
	{
		var typeSymbol = GetTypeSymbol(@"
                public class TestClass<T>
                {
                    public TestClass() { }
                    public virtual void DoSomething(T value) { }
                }
            ", "TestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	[Fact]
	public void CanBeMocked_AbstractGenericClass_ShouldReturnTrue()
	{
		var typeSymbol = GetTypeSymbol(@"
                public abstract class TestClass<T>
                {
                    public abstract void DoSomething(T value);
                }
            ", "TestClass");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	[Fact]
	public void CanBeMocked_InterfaceWithMultipleMembers_ShouldReturnTrue()
	{
		var typeSymbol = GetTypeSymbol(@"
                public interface ITestInterface
                {
                    void DoSomething();
                    string GetValue();
                    int Count { get; set; }
                    event System.EventHandler Changed;
                }
            ", "ITestInterface");

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	#endregion

	#region Helper Methods

	private INamedTypeSymbol GetTypeSymbol(string source, string typeName)
	{
		var compilation = CreateCompilation(source);
		var typeSymbol = compilation.GetTypeByMetadataName(typeName);

		if (typeSymbol == null)
		{
			// For nested types, we need to search differently
			typeSymbol = compilation.GetSymbolsWithName(typeName, SymbolFilter.Type)
				.OfType<INamedTypeSymbol>()
				.FirstOrDefault();
		}

		typeSymbol.Should().NotBeNull($"Type '{typeName}' should be found in compilation");
		return typeSymbol!;
	}

	private CSharpCompilation CreateCompilation(string source)
	{
		var syntaxTree = CSharpSyntaxTree.ParseText(source);

		var references = new[]
		{
			MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(System.EventHandler).Assembly.Location),
		};

		return CSharpCompilation.Create(
			"TestAssembly",
			new[] { syntaxTree },
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	}

	#endregion
}