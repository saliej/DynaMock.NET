using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using DynaMock.SourceGeneration.Models;
using Microsoft.CodeAnalysis;

namespace DynaMock.SourceGeneration;

/// <summary>
/// Extracts types to mock from attribute syntax context
/// </summary>
public class AttributeTypesExtractor
{
	public ImmutableArray<TypeToMock> ExtractTypesToMock(
		GeneratorAttributeSyntaxContext context,
		string attributeFullName)
	{
		if (context.TargetSymbol is not INamedTypeSymbol)
			return ImmutableArray<TypeToMock>.Empty;

		var attribute = context.Attributes
			.FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == attributeFullName);

		if (attribute is null)
			return ImmutableArray<TypeToMock>.Empty;

		if (attribute.ConstructorArguments.Length == 0)
			return ImmutableArray<TypeToMock>.Empty;

		var virtualMembers = ExtractVirtualMembersFlag(attribute);
		var typesToMock = ImmutableArray.CreateBuilder<TypeToMock>();

		var firstArg = attribute.ConstructorArguments[0];
		ProcessConstructorArgument(firstArg, context.TargetNode.GetLocation(), virtualMembers, typesToMock);

		return typesToMock.ToImmutable();
	}

	private bool ExtractVirtualMembersFlag(AttributeData attribute)
	{
		foreach (var namedArg in attribute.NamedArguments)
		{
			if (namedArg.Key == "VirtualMembers" && namedArg.Value.Value is bool value)
				return value;
		}
		return false;
	}

	private void ProcessConstructorArgument(
		TypedConstant firstArg,
		Location location,
		bool virtualMembers,
		ImmutableArray<TypeToMock>.Builder typesToMock)
	{
		if (firstArg.Kind == TypedConstantKind.Array)
		{
			foreach (var value in firstArg.Values)
			{
				if (value.Value is INamedTypeSymbol namedType)
					typesToMock.Add(new TypeToMock(namedType, location, virtualMembers));
			}
		}
		else if (firstArg.Value is INamedTypeSymbol singleType)
		{
			typesToMock.Add(new TypeToMock(singleType, location, virtualMembers));
		}
	}
}