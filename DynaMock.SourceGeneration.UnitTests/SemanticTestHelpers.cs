using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace DynaMock.SourceGeneration.UnitTests;

/// <summary>
/// Extension methods for semantic testing of generated code using Roslyn.
/// Unlike snapshot tests that check exact text output, these helpers inspect
/// the semantic model to verify structure and relationships.
/// </summary>
public static class SemanticTestHelpers
{
    /// <summary>
    /// Asserts that a type exists in the compilation.
    /// </summary>
    public static void AssertTypeExists(this Compilation compilation, string typeName)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);
    }

    /// <summary>
    /// Asserts that a type implements a specific interface.
    /// </summary>
    public static void AssertTypeImplements(this Compilation compilation, string typeName, string interfaceName)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);

        var interfaceType = compilation.GetTypeByMetadataName(interfaceName);
        Assert.NotNull(interfaceType);

        var implements = type.AllInterfaces.Any(i => i.Equals(interfaceType));
        Assert.True(implements);
    }

    /// <summary>
    /// Asserts that a type extends a specific base class.
    /// </summary>
    public static void AssertTypeExtends(this Compilation compilation, string typeName, string baseTypeName)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);

        var baseType = compilation.GetTypeByMetadataName(baseTypeName);
        Assert.NotNull(baseType);

        Assert.Equal(baseType, type.BaseType);
    }

    /// <summary>
    /// Asserts that a type has a specific method.
    /// </summary>
    public static void AssertHasMethod(this Compilation compilation, string typeName, string methodName)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);

        var method = type.GetMembers(methodName).FirstOrDefault();
        Assert.NotNull(method);
    }

    /// <summary>
    /// Asserts that a type has a specific property.
    /// </summary>
    public static void AssertHasProperty(this Compilation compilation, string typeName, string propertyName)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);

        var property = type.GetMembers(propertyName).FirstOrDefault();
        Assert.NotNull(property);
    }

    /// <summary>
    /// Asserts that a type has a specific event.
    /// </summary>
    public static void AssertHasEvent(this Compilation compilation, string typeName, string eventName)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);

        var eventMember = type.GetMembers(eventName).FirstOrDefault();
        Assert.NotNull(eventMember);
    }

    /// <summary>
    /// Asserts that a type has a specific field.
    /// </summary>
    public static void AssertHasField(this Compilation compilation, string typeName, string fieldName)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);

        var field = type.GetMembers(fieldName).FirstOrDefault();
        Assert.NotNull(field);
    }

    /// <summary>
    /// Asserts that a type is in the expected namespace.
    /// </summary>
    public static void AssertInNamespace(this Compilation compilation, string typeName, string expectedNamespace)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);

        var actualNamespace = type.ContainingNamespace.ToDisplayString();
        Assert.Equal(expectedNamespace, actualNamespace);
    }

    /// <summary>
    /// Asserts that a type has the expected accessibility.
    /// </summary>
    public static void AssertAccessibility(this Compilation compilation, string typeName, Accessibility expectedAccessibility)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);

        var actualAccessibility = type.DeclaredAccessibility;
        Assert.Equal(expectedAccessibility, actualAccessibility);
    }

    /// <summary>
    /// Asserts that a type is abstract.
    /// </summary>
    public static void AssertIsAbstract(this Compilation compilation, string typeName)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);

        Assert.True(type.IsAbstract);
    }

    /// <summary>
    /// Asserts that a type is sealed.
    /// </summary>
    public static void AssertIsSealed(this Compilation compilation, string typeName)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);

        Assert.True(type.IsSealed);
    }

    /// <summary>
    /// Asserts that a type is static.
    /// </summary>
    public static void AssertIsStatic(this Compilation compilation, string typeName)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);

        Assert.True(type.IsStatic);
    }
}
