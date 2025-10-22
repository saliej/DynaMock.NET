using System;
using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using NSubstitute;


namespace DynaMock.SourceGeneration.UnitTests;

public class MockableGeneratorIntegrationTests
{
	[Fact]
	public void FullPipeline_ValidatesInvalidType_ReturnsAppropriateResult()
	{
		var validator = new TypeValidator();
		var sealedType = CreateSealedClass();

		var canBeMocked = validator.CanBeMocked(sealedType, out var reason);

		canBeMocked.Should().BeFalse();
		reason.Should().Contain("sealed");
	}

	[Fact]
	public void FullPipeline_BuildsAndGeneratesCode_ProducesValidOutput()
	{
		var validator = new TypeValidator();
		var modelBuilder = new TypeModelBuilder();
		var codeGenerator = new WrapperCodeGenerator();

		var typeSymbol = CreateValidInterfaceType();

		var isValid = validator.CanBeMocked(typeSymbol, out _);
		var model = modelBuilder.BuildTypeModel(typeSymbol);
		var code = codeGenerator.GenerateWrapperClass(model, virtualMembers: false);

		isValid.Should().BeTrue();
		model.Should().NotBeNull();
		code.Should().Contain("namespace DynaMock.Generated");
		code.Should().Contain("public class MockableITestService");
	}

	private INamedTypeSymbol CreateSealedClass()
	{
		var symbol = Substitute.For<INamedTypeSymbol>();
		symbol.TypeKind.Returns(TypeKind.Class);
		symbol.IsSealed.Returns(true);
		symbol.SpecialType.Returns(SpecialType.None);
		return symbol;
	}

	private INamedTypeSymbol CreateValidInterfaceType()
	{
		var symbolArray = ImmutableArray.Create(new ISymbol[]
		{
			CreateMethod("GetValue", "string")
		});

		var symbol = Substitute.For<INamedTypeSymbol>();
		symbol.Name.Returns("ITestService");
		symbol.TypeKind.Returns(TypeKind.Interface);
		symbol.IsSealed.Returns(false);
		symbol.IsStatic.Returns(false);
		symbol.SpecialType.Returns(SpecialType.None);
		symbol.TypeParameters.Returns(ImmutableArray<ITypeParameterSymbol>.Empty);
		symbol.GetMembers().Returns(symbolArray);

		var ns = Substitute.For<INamespaceSymbol>();
		ns.IsGlobalNamespace.Returns(false);
		ns.ToDisplayString().Returns("Test.Namespace");
		symbol.ContainingNamespace.Returns(ns);

		return symbol;
	}

	private IMethodSymbol CreateMethod(string name, string returnType)
	{
		var method = Substitute.For<IMethodSymbol>();
		method.Name.Returns(name);
		method.MethodKind.Returns(MethodKind.Ordinary);
		method.Parameters.Returns(ImmutableArray<IParameterSymbol>.Empty);
		method.TypeParameters.Returns(ImmutableArray<ITypeParameterSymbol>.Empty);

		var retType = Substitute.For<ITypeSymbol>();
		retType.ToDisplayString().Returns(returnType);
		method.ReturnType.Returns(retType);

		var containingType = Substitute.For<INamedTypeSymbol>();
		containingType.TypeKind.Returns(TypeKind.Interface);
		method.ContainingType.Returns(containingType);

		return method;
	}
}
