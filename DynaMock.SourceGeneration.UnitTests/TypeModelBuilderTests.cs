using System;
using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using NSubstitute;

namespace DynaMock.SourceGeneration.UnitTests;

public class TypeModelBuilderTests
{
	private readonly TypeModelBuilder _builder;

	public TypeModelBuilderTests()
	{
		_builder = new TypeModelBuilder();
	}

	[Fact]
	public void BuildTypeModel_WithNullSymbol_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => _builder.BuildTypeModel(null!));
	}

	[Fact]
	public void BuildTypeModel_WithInterface_CreatesInterfaceModel()
	{
		var typeSymbol = CreateTypeSymbol("ITestService", TypeKind.Interface, "TestNamespace");

		var result = _builder.BuildTypeModel(typeSymbol);

		result.Should().NotBeNull();
		result.Name.Should().Be("ITestService");
		result.Namespace.Should().Be("TestNamespace");
		result.IsInterface.Should().BeTrue();
	}

	[Fact]
	public void BuildTypeModel_WithClass_CreatesClassModel()
	{
		var typeSymbol = CreateTypeSymbol("TestService", TypeKind.Class, "TestNamespace");

		var result = _builder.BuildTypeModel(typeSymbol);

		result.Name.Should().Be("TestService");
		result.IsInterface.Should().BeFalse();
	}

	[Fact]
	public void BuildTypeModel_WithGlobalNamespace_HasEmptyNamespace()
	{
		var typeSymbol = CreateTypeSymbol("TestService", TypeKind.Class, isGlobalNamespace: true);

		var result = _builder.BuildTypeModel(typeSymbol);

		result.Namespace.Should().BeEmpty();
	}

	[Fact]
	public void BuildTypeModel_WithMethods_IncludesMethodsInModel()
	{
		var method = CreateMethodMember("GetValue", MethodKind.Ordinary);
		var typeSymbol = CreateTypeSymbol("TestService", TypeKind.Class, members: new ISymbol[] { method });

		var result = _builder.BuildTypeModel(typeSymbol);

		result.Methods.Should().HaveCount(1);
		result.Methods[0].Name.Should().Be("GetValue");
	}

	[Fact]
	public void BuildTypeModel_WithProperties_IncludesPropertiesInModel()
	{
		var property = CreatePropertyMember("Name");
		var typeSymbol = CreateTypeSymbol("TestService", TypeKind.Class, members: new ISymbol[] { property });

		var result = _builder.BuildTypeModel(typeSymbol);

		result.Properties.Should().HaveCount(1);
		result.Properties[0].Name.Should().Be("Name");
	}

	[Fact]
	public void BuildTypeModel_WithEvents_IncludesEventsInModel()
	{
		var evt = CreateEventMember("Changed");
		var typeSymbol = CreateTypeSymbol("TestService", TypeKind.Class, members: new ISymbol[] { evt });

		var result = _builder.BuildTypeModel(typeSymbol);

		result.Events.Should().HaveCount(1);
		result.Events[0].Name.Should().Be("Changed");
	}

	[Fact]
	public void BuildTypeModel_WithGenericType_IncludesTypeParameters()
	{
		var typeParam = CreateTypeParameter("T");
		var typeSymbol = CreateTypeSymbol("TestService", TypeKind.Class, typeParameters: new[] { typeParam });

		var result = _builder.BuildTypeModel(typeSymbol);

		result.GenericTypeParameters.Should().Contain("T");
	}

	[Fact]
	public void BuildTypeModel_WithProblematicMember_SkipsMemberAndContinues()
	{
		// Create a method that will throw when accessed
		var goodMethod = CreateMethodMember("GoodMethod", MethodKind.Ordinary);
		var badProperty = Substitute.For<IPropertySymbol>();
		badProperty.Name.Returns(_ => throw new InvalidOperationException("Test exception"));
		var anotherGoodMethod = CreateMethodMember("AnotherMethod", MethodKind.Ordinary);

		var typeSymbol = CreateTypeSymbol("TestService", TypeKind.Class,
			members: new ISymbol[] { goodMethod, badProperty, anotherGoodMethod });

		var result = _builder.BuildTypeModel(typeSymbol);

		// Should have both good methods but skip the bad property
		result.Methods.Should().HaveCount(2);
		result.Properties.Should().BeEmpty();
	}

	// Helper methods
	private INamedTypeSymbol CreateTypeSymbol(
		string name,
		TypeKind typeKind,
		string? namespaceName = null,
		bool isGlobalNamespace = false,
		ISymbol[]? members = null,
		ITypeParameterSymbol[]? typeParameters = null)
	{
		var membersArray = members != null
			? ImmutableArray.Create(members)
			: ImmutableArray<ISymbol>.Empty;

		var typeParametersArray = typeParameters != null
			? ImmutableArray.Create(typeParameters)
			: ImmutableArray<ITypeParameterSymbol>.Empty;

		var symbol = Substitute.For<INamedTypeSymbol>();
		symbol.Name.Returns(name);
		symbol.TypeKind.Returns(typeKind);
		symbol.GetMembers().Returns(membersArray);
		symbol.TypeParameters.Returns(typeParametersArray);

		if (isGlobalNamespace)
		{
			var globalNs = Substitute.For<INamespaceSymbol>();
			globalNs.IsGlobalNamespace.Returns(true);
			symbol.ContainingNamespace.Returns(globalNs);
		}
		else if (namespaceName != null)
		{
			var ns = Substitute.For<INamespaceSymbol>();
			ns.IsGlobalNamespace.Returns(false);
			ns.ToDisplayString().Returns(namespaceName);
			symbol.ContainingNamespace.Returns(ns);
		}

		return symbol;
	}

	private IMethodSymbol CreateMethodMember(string name, MethodKind methodKind)
	{
		var method = Substitute.For<IMethodSymbol>();
		method.Name.Returns(name);
		method.MethodKind.Returns(methodKind);
		method.Parameters.Returns(ImmutableArray<IParameterSymbol>.Empty);
		method.TypeParameters.Returns(ImmutableArray<ITypeParameterSymbol>.Empty);

		var returnType = Substitute.For<ITypeSymbol>();
		returnType.ToDisplayString().Returns("void");
		method.ReturnType.Returns(returnType);

		var containingType = Substitute.For<INamedTypeSymbol>();
		containingType.TypeKind.Returns(TypeKind.Class);
		method.ContainingType.Returns(containingType);

		return method;
	}

	private IPropertySymbol CreatePropertyMember(string name)
	{
		var property = Substitute.For<IPropertySymbol>();
		property.Name.Returns(name);

		var type = Substitute.For<ITypeSymbol>();
		type.ToDisplayString().Returns("string");
		property.Type.Returns(type);

		property.GetMethod.Returns(Substitute.For<IMethodSymbol>());
		property.SetMethod.Returns(Substitute.For<IMethodSymbol>());

		return property;
	}

	private IEventSymbol CreateEventMember(string name)
	{
		var evt = Substitute.For<IEventSymbol>();
		evt.Name.Returns(name);

		var type = Substitute.For<ITypeSymbol>();
		type.ToDisplayString().Returns("EventHandler");
		evt.Type.Returns(type);

		return evt;
	}

	private ITypeParameterSymbol CreateTypeParameter(string name)
	{
		var param = Substitute.For<ITypeParameterSymbol>();
		param.Name.Returns(name);
		param.HasReferenceTypeConstraint.Returns(false);
		param.HasValueTypeConstraint.Returns(false);
		param.HasNotNullConstraint.Returns(false);
		param.HasUnmanagedTypeConstraint.Returns(false);
		param.HasConstructorConstraint.Returns(false);
		param.ConstraintTypes.Returns(ImmutableArray<ITypeSymbol>.Empty);

		return param;
	}
}