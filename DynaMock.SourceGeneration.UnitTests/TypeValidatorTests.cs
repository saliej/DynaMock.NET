using System;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using NSubstitute;

namespace DynaMock.SourceGeneration.UnitTests;

public class TypeValidatorTests
{
	private readonly TypeValidator _validator;

	public TypeValidatorTests()
	{
		_validator = new TypeValidator();
	}

	[Fact]
	public void CanBeMocked_WithRegularClass_ReturnsTrue()
	{
		var typeSymbol = CreateTypeSymbol(TypeKind.Class, isSealed: false, isStatic: false);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	[Fact]
	public void CanBeMocked_WithInterface_ReturnsTrue()
	{
		var typeSymbol = CreateTypeSymbol(TypeKind.Interface);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	[Fact]
	public void CanBeMocked_WithSealedClass_ReturnsFalse()
	{
		var typeSymbol = CreateTypeSymbol(TypeKind.Class, isSealed: true);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("sealed classes cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_WithStaticClass_ReturnsFalse()
	{
		var typeSymbol = CreateTypeSymbol(TypeKind.Class, isStatic: true);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("static classes cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_WithDelegate_ReturnsFalse()
	{
		var typeSymbol = CreateTypeSymbol(TypeKind.Delegate);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("delegates cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_WithEnum_ReturnsFalse()
	{
		var typeSymbol = CreateTypeSymbol(TypeKind.Enum);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("enums cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_WithStruct_ReturnsFalse()
	{
		var typeSymbol = CreateTypeSymbol(TypeKind.Struct);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("structs cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_WithPrimitiveType_ReturnsFalse()
	{
		var typeSymbol = CreateTypeSymbol(TypeKind.Class, specialType: SpecialType.System_Int32);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeFalse();
		reason.Should().Be("primitive types cannot be mocked");
	}

	[Fact]
	public void CanBeMocked_WithSystemObject_ReturnsTrue()
	{
		var typeSymbol = CreateTypeSymbol(TypeKind.Class, specialType: SpecialType.System_Object);

		var result = _validator.CanBeMocked(typeSymbol, out var reason);

		result.Should().BeTrue();
		reason.Should().BeEmpty();
	}

	private INamedTypeSymbol CreateTypeSymbol(
		TypeKind typeKind,
		bool isSealed = false,
		bool isStatic = false,
		SpecialType specialType = SpecialType.None)
	{
		var symbol = Substitute.For<INamedTypeSymbol>();
		symbol.TypeKind.Returns(typeKind);
		symbol.IsSealed.Returns(isSealed);
		symbol.IsStatic.Returns(isStatic);
		symbol.SpecialType.Returns(specialType);
		return symbol;
	}
}