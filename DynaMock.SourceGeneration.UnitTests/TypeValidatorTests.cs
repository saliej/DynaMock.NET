using System;
using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using NSubstitute;

namespace DynaMock.SourceGeneration.UnitTests;

public class TypeValidatorTests
{
    private readonly TypeValidator _sut = new();

    [Fact]
    public void CanBeMocked_SealedClass_ReturnsFalse()
    {
        var typeSymbol = CreateTypeSymbol(TypeKind.Class, isSealed: true);

        var result = _sut.CanBeMocked(typeSymbol, out var reason);

        result.Should().BeFalse();
        reason.Should().Be("sealed classes cannot be mocked");
    }

    [Fact]
    public void CanBeMocked_StaticClass_ReturnsFalse()
    {
        var typeSymbol = CreateTypeSymbol(TypeKind.Class, isStatic: true);

        var result = _sut.CanBeMocked(typeSymbol, out var reason);

        result.Should().BeFalse();
        reason.Should().Be("static classes cannot be mocked");
    }

    [Fact]
    public void CanBeMocked_PrimitiveType_ReturnsFalse()
    {
        var typeSymbol = CreateTypeSymbol(TypeKind.Class, specialType: SpecialType.System_Int32);

        var result = _sut.CanBeMocked(typeSymbol, out var reason);

        result.Should().BeFalse();
        reason.Should().Be("primitive types cannot be mocked");
    }

    [Fact]
    public void CanBeMocked_Delegate_ReturnsFalse()
    {
        var typeSymbol = CreateTypeSymbol(TypeKind.Delegate);

        var result = _sut.CanBeMocked(typeSymbol, out var reason);

        result.Should().BeFalse();
        reason.Should().Be("delegates cannot be mocked");
    }

    [Fact]
    public void CanBeMocked_Enum_ReturnsFalse()
    {
        var typeSymbol = CreateTypeSymbol(TypeKind.Enum);

        var result = _sut.CanBeMocked(typeSymbol, out var reason);

        result.Should().BeFalse();
        reason.Should().Be("enums cannot be mocked");
    }

    [Fact]
    public void CanBeMocked_Struct_ReturnsFalse()
    {
        var typeSymbol = CreateTypeSymbol(TypeKind.Struct);

        var result = _sut.CanBeMocked(typeSymbol, out var reason);

        result.Should().BeFalse();
        reason.Should().Be("structs cannot be mocked");
    }

    [Fact]
    public void CanBeMocked_ConcreteClassWithoutParameterlessConstructor_ReturnsFalse()
    {
        var constructor = Substitute.For<IMethodSymbol>();
        constructor.Parameters.Returns(ImmutableArray.Create(Substitute.For<IParameterSymbol>()));
        constructor.DeclaredAccessibility.Returns(Accessibility.Public);

        var typeSymbol = CreateTypeSymbol(TypeKind.Class, isAbstract: false, constructors: new[] { constructor });

        var result = _sut.CanBeMocked(typeSymbol, out var reason);

        result.Should().BeFalse();
        reason.Should().Be("concrete classes must have a public parameterless constructor");
    }

    [Fact]
    public void CanBeMocked_ConcreteClassWithPrivateParameterlessConstructor_ReturnsFalse()
    {
        var constructor = Substitute.For<IMethodSymbol>();
        constructor.Parameters.Returns(ImmutableArray<IParameterSymbol>.Empty);
        constructor.DeclaredAccessibility.Returns(Accessibility.Private);

        var typeSymbol = CreateTypeSymbol(TypeKind.Class, isAbstract: false, constructors: new[] { constructor });

        var result = _sut.CanBeMocked(typeSymbol, out var reason);

        result.Should().BeFalse();
        reason.Should().Be("concrete classes must have a public parameterless constructor");
    }

    [Fact]
    public void CanBeMocked_ConcreteClassWithPublicParameterlessConstructor_ReturnsTrue()
    {
        var constructor = Substitute.For<IMethodSymbol>();
        constructor.Parameters.Returns(ImmutableArray<IParameterSymbol>.Empty);
        constructor.DeclaredAccessibility.Returns(Accessibility.Public);

        var typeSymbol = CreateTypeSymbol(TypeKind.Class, isAbstract: false, constructors: new[] { constructor });

        var result = _sut.CanBeMocked(typeSymbol, out var reason);

        result.Should().BeTrue();
        reason.Should().BeEmpty();
    }

    [Fact]
    public void CanBeMocked_AbstractClass_ReturnsTrue()
    {
        var typeSymbol = CreateTypeSymbol(TypeKind.Class, isAbstract: true);

        var result = _sut.CanBeMocked(typeSymbol, out var reason);

        result.Should().BeTrue();
        reason.Should().BeEmpty();
    }

    [Fact]
    public void CanBeMocked_Interface_ReturnsTrue()
    {
        var typeSymbol = CreateTypeSymbol(TypeKind.Interface);

        var result = _sut.CanBeMocked(typeSymbol, out var reason);

        result.Should().BeTrue();
        reason.Should().BeEmpty();
    }

    [Fact]
    public void CanBeMocked_SystemObject_ReturnsTrue()
    {
        var constructor = Substitute.For<IMethodSymbol>();
        constructor.Parameters.Returns(ImmutableArray<IParameterSymbol>.Empty);
        constructor.DeclaredAccessibility.Returns(Accessibility.Public);

        var typeSymbol = CreateTypeSymbol(
            TypeKind.Class,
            specialType: SpecialType.System_Object,
            isAbstract: false,
            constructors: new[] { constructor });

        var result = _sut.CanBeMocked(typeSymbol, out var reason);

        result.Should().BeTrue();
        reason.Should().BeEmpty();
    }

    private static INamedTypeSymbol CreateTypeSymbol(
        TypeKind typeKind,
        bool isSealed = false,
        bool isStatic = false,
        bool isAbstract = false,
        SpecialType specialType = SpecialType.None,
        IMethodSymbol[] constructors = null)
    {
        var symbol = Substitute.For<INamedTypeSymbol>();
        symbol.TypeKind.Returns(typeKind);
        symbol.IsSealed.Returns(isSealed);
        symbol.IsStatic.Returns(isStatic);
        symbol.IsAbstract.Returns(isAbstract);
        symbol.SpecialType.Returns(specialType);
        symbol.Constructors.Returns(constructors != null 
            ? ImmutableArray.Create(constructors) 
            : ImmutableArray<IMethodSymbol>.Empty);
        return symbol;
    }
}