using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace DynaMock.SourceGeneration;

/// <summary>
/// Validates whether types can be mocked
/// </summary>
public class TypeValidator
{
	public bool CanBeMocked(INamedTypeSymbol typeSymbol, out string reason)
	{
		reason = string.Empty;

		if (typeSymbol.TypeKind == TypeKind.Class && typeSymbol.IsSealed)
		{
			reason = "sealed classes cannot be mocked";
			return false;
		}

		if (typeSymbol.IsStatic)
		{
			reason = "static classes cannot be mocked";
			return false;
		}

		if (typeSymbol.SpecialType != SpecialType.None &&
			typeSymbol.SpecialType != SpecialType.System_Object)
		{
			reason = "primitive types cannot be mocked";
			return false;
		}

		if (typeSymbol.TypeKind == TypeKind.Delegate)
		{
			reason = "delegates cannot be mocked";
			return false;
		}

		if (typeSymbol.TypeKind == TypeKind.Enum)
		{
			reason = "enums cannot be mocked";
			return false;
		}

		if (typeSymbol.TypeKind == TypeKind.Struct)
		{
			reason = "structs cannot be mocked";
			return false;
		}

		// For concrete classes (not abstract), require parameterless constructor
		if (typeSymbol.TypeKind == TypeKind.Class && !typeSymbol.IsAbstract)
		{
			var hasParameterlessConstructor = typeSymbol.Constructors
				.Any(c => c.Parameters.Length == 0 &&
						 c.DeclaredAccessibility == Accessibility.Public);

			if (!hasParameterlessConstructor)
			{
				reason = "concrete classes must have a public parameterless constructor";
				return false;
			}
		}

		return true;
	}
}