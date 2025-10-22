using System;
using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using NSubstitute;

namespace DynaMock.SourceGeneration.UnitTests;

public class MemberModelBuilderTests
{
	private readonly MemberModelBuilder _builder;

	public MemberModelBuilderTests()
	{
		_builder = new MemberModelBuilder();
	}

	[Fact]
	public void BuildMethodModel_WithNullSymbol_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => _builder.BuildMethodModel(null!));
	}

	[Fact]
	public void BuildMethodModel_WithRegularMethod_ReturnsValidModel()
	{
		var methodSymbol = CreateMethodSymbol(
			name: "GetValue",
			returnType: "string",
			isVirtual: false,
			isAbstract: false);

		var result = _builder.BuildMethodModel(methodSymbol);

		result.Should().NotBeNull();
		result.Name.Should().Be("GetValue");
		result.ReturnType.Should().Be("string");
		result.IsVirtual.Should().BeFalse();
		result.IsAbstract.Should().BeFalse();
	}

	[Fact]
	public void BuildMethodModel_WithVirtualMethod_HasNoBaseImplementation()
	{
		var methodSymbol = CreateMethodSymbol(
			name: "GetValue",
			returnType: "string",
			isVirtual: true);

		var result = _builder.BuildMethodModel(methodSymbol);

		result.BaseImplementation.Should().BeNull();
	}

	[Fact]
	public void BuildMethodModel_WithAbstractMethod_HasNoBaseImplementation()
	{
		var methodSymbol = CreateMethodSymbol(
			name: "GetValue",
			returnType: "string",
			isAbstract: true);

		var result = _builder.BuildMethodModel(methodSymbol);

		result.BaseImplementation.Should().BeNull();
	}

	[Fact]
	public void BuildMethodModel_WithParameters_IncludesParametersInModel()
	{
		var param1 = CreateParameter("string", "name");
		var param2 = CreateParameter("int", "age");
		var methodSymbol = CreateMethodSymbol(
			name: "UpdateUser",
			returnType: "void",
			parameters: new[] { param1, param2 });

		var result = _builder.BuildMethodModel(methodSymbol);

		result.Parameters.Should().HaveCount(2);
		result.Parameters[0].Should().Be(("string", "name"));
		result.Parameters[1].Should().Be(("int", "age"));
	}

	[Fact]
	public void BuildMethodModel_WithTaskReturnType_IsMarkedAsAsync()
	{
		var returnType = CreateNamedTypeSymbol("Task");
		var methodSymbol = CreateMethodSymbol(
			name: "GetValueAsync",
			returnTypeSymbol: returnType);

		var result = _builder.BuildMethodModel(methodSymbol);

		result.IsAsync.Should().BeTrue();
	}

	[Fact]
	public void BuildMethodModel_WithValueTaskReturnType_IsMarkedAsAsync()
	{
		var returnType = CreateNamedTypeSymbol("ValueTask");
		var methodSymbol = CreateMethodSymbol(
			name: "GetValueAsync",
			returnTypeSymbol: returnType);

		var result = _builder.BuildMethodModel(methodSymbol);

		result.IsAsync.Should().BeTrue();
	}

	[Fact]
	public void BuildMethodModel_WithGenericTaskReturnType_IsMarkedAsAsync()
	{
		var returnType = CreateGenericNamedTypeSymbol("Task", isGeneric: true);
		var methodSymbol = CreateMethodSymbol(
			name: "GetValueAsync",
			returnTypeSymbol: returnType);

		var result = _builder.BuildMethodModel(methodSymbol);

		result.IsAsync.Should().BeTrue();
	}

	[Fact]
	public void BuildPropertyModel_WithNullSymbol_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => _builder.BuildPropertyModel(null!));
	}

	[Fact]
	public void BuildPropertyModel_WithGetterAndSetter_ReturnsValidModel()
	{
		var propertySymbol = CreatePropertySymbol(
			name: "Name",
			type: "string",
			hasGetter: true,
			hasSetter: true);

		var result = _builder.BuildPropertyModel(propertySymbol);

		result.Should().NotBeNull();
		result.Name.Should().Be("Name");
		result.Type.Should().Be("string");
		result.HasGetter.Should().BeTrue();
		result.HasSetter.Should().BeTrue();
	}

	[Fact]
	public void BuildPropertyModel_WithGetterOnly_HasNoSetter()
	{
		var propertySymbol = CreatePropertySymbol(
			name: "Id",
			type: "int",
			hasGetter: true,
			hasSetter: false);

		var result = _builder.BuildPropertyModel(propertySymbol);

		result.HasGetter.Should().BeTrue();
		result.HasSetter.Should().BeFalse();
	}

	[Fact]
	public void BuildPropertyModel_WithVirtualProperty_MarkedAsVirtual()
	{
		var propertySymbol = CreatePropertySymbol(
			name: "Name",
			type: "string",
			isVirtual: true);

		var result = _builder.BuildPropertyModel(propertySymbol);

		result.IsVirtual.Should().BeTrue();
	}

	[Fact]
	public void BuildEventModel_WithNullSymbol_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => _builder.BuildEventModel(null!));
	}

	[Fact]
	public void BuildEventModel_WithValidEvent_ReturnsValidModel()
	{
		var eventSymbol = CreateEventSymbol(
			name: "Changed",
			type: "EventHandler");

		var result = _builder.BuildEventModel(eventSymbol);

		result.Should().NotBeNull();
		result.Name.Should().Be("Changed");
		result.Type.Should().Be("EventHandler");
	}

	[Fact]
	public void BuildEventModel_WithVirtualEvent_MarkedAsVirtual()
	{
		var eventSymbol = CreateEventSymbol(
			name: "Changed",
			type: "EventHandler",
			isVirtual: true);

		var result = _builder.BuildEventModel(eventSymbol);

		result.IsVirtual.Should().BeTrue();
	}

	// Helper methods for creating test doubles
	private IMethodSymbol CreateMethodSymbol(
		string name,
		string returnType = "void",
		ITypeSymbol? returnTypeSymbol = null,
		bool isVirtual = false,
		bool isAbstract = false,
		IParameterSymbol[]? parameters = null)
	{
		var parametersArray  = parameters != null
			? ImmutableArray.Create(parameters)
			: ImmutableArray<IParameterSymbol>.Empty;

		var symbol = Substitute.For<IMethodSymbol>();
		symbol.Name.Returns(name);
		symbol.IsVirtual.Returns(isVirtual);
		symbol.IsAbstract.Returns(isAbstract);
		symbol.Parameters.Returns(parametersArray);

		if (returnTypeSymbol != null)
		{
			symbol.ReturnType.Returns(returnTypeSymbol);
		}
		else
		{
			var retType = Substitute.For<ITypeSymbol>();
			retType.ToDisplayString().Returns(returnType);
			symbol.ReturnType.Returns(retType);
		}

		symbol.TypeParameters.Returns(ImmutableArray<ITypeParameterSymbol>.Empty);

		var containingType = Substitute.For<INamedTypeSymbol>();
		containingType.TypeKind.Returns(TypeKind.Class);
		symbol.ContainingType.Returns(containingType);

		return symbol;
	}

	private IParameterSymbol CreateParameter(string type, string name)
	{
		var param = Substitute.For<IParameterSymbol>();
		param.Name.Returns(name);

		var typeSymbol = Substitute.For<ITypeSymbol>();
		typeSymbol.ToDisplayString().Returns(type);
		param.Type.Returns(typeSymbol);

		return param;
	}

	private INamedTypeSymbol CreateNamedTypeSymbol(string name)
	{
		var symbol = Substitute.For<INamedTypeSymbol>();
		symbol.Name.Returns(name);
		symbol.IsGenericType.Returns(false);

		return symbol;
	}

	private INamedTypeSymbol CreateGenericNamedTypeSymbol(string name, bool isGeneric)
	{
		var symbol = Substitute.For<INamedTypeSymbol>();
		symbol.Name.Returns(name);
		symbol.IsGenericType.Returns(isGeneric);

		if (isGeneric)
		{
			var constructedFrom = Substitute.For<INamedTypeSymbol>();
			constructedFrom.Name.Returns(name);
			symbol.ConstructedFrom.Returns(constructedFrom);
		}

		return symbol;
	}

	private IPropertySymbol CreatePropertySymbol(
		string name,
		string type,
		bool hasGetter = true,
		bool hasSetter = true,
		bool isVirtual = false,
		bool isAbstract = false)
	{
		var symbol = Substitute.For<IPropertySymbol>();
		symbol.Name.Returns(name);
		symbol.IsVirtual.Returns(isVirtual);
		symbol.IsAbstract.Returns(isAbstract);

		var typeSymbol = Substitute.For<ITypeSymbol>();
		typeSymbol.ToDisplayString().Returns(type);
		symbol.Type.Returns(typeSymbol);

		symbol.GetMethod.Returns(hasGetter ? Substitute.For<IMethodSymbol>() : null);
		symbol.SetMethod.Returns(hasSetter ? Substitute.For<IMethodSymbol>() : null);

		return symbol;
	}

	private IEventSymbol CreateEventSymbol(
		string name,
		string type,
		bool isVirtual = false,
		bool isAbstract = false)
	{
		var symbol = Substitute.For<IEventSymbol>();
		symbol.Name.Returns(name);
		symbol.IsVirtual.Returns(isVirtual);
		symbol.IsAbstract.Returns(isAbstract);

		var typeSymbol = Substitute.For<ITypeSymbol>();
		typeSymbol.ToDisplayString().Returns(type);
		symbol.Type.Returns(typeSymbol);

		return symbol;
	}
}