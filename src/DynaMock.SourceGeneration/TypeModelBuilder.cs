using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using DynaMock.SourceGeneration.Models;

namespace DynaMock.SourceGeneration;

/// <summary>
/// Builds type models from Roslyn symbols
/// </summary>
public class TypeModelBuilder
{
	private readonly MemberModelBuilder _memberBuilder;

	public TypeModelBuilder()
	{
		_memberBuilder = new MemberModelBuilder();
	}

	public TypeModel BuildTypeModel(INamedTypeSymbol typeSymbol)
	{
		if (typeSymbol is null)
			throw new ArgumentNullException(nameof(typeSymbol));

		var model = new TypeModel
		{
			Name = typeSymbol.Name,
			Namespace = GetNamespace(typeSymbol),
			IsInterface = typeSymbol.TypeKind == TypeKind.Interface,
			GenericTypeParameters = typeSymbol.TypeParameters.Select(tp => tp.Name).ToList(),
			GenericTypeConstraints = typeSymbol.TypeParameters
				.Select(GetTypeParameterConstraints)
				.Where(c => !string.IsNullOrEmpty(c))
				.ToList()
		};

		PopulateMembers(typeSymbol, model);
		return model;
	}

	private string GetNamespace(INamedTypeSymbol typeSymbol)
	{
		var containingNamespace = typeSymbol.ContainingNamespace;
		return containingNamespace?.IsGlobalNamespace == false
			? containingNamespace.ToDisplayString()
			: string.Empty;
	}

	private void PopulateMembers(INamedTypeSymbol typeSymbol, TypeModel model)
	{
		foreach (var member in typeSymbol.GetMembers())
		{
			try
			{
				switch (member)
				{
					case IMethodSymbol methodSymbol when IsRegularMethod(methodSymbol):
						model.Methods.Add(_memberBuilder.BuildMethodModel(methodSymbol));
						break;
					case IPropertySymbol propertySymbol:
						model.Properties.Add(_memberBuilder.BuildPropertyModel(propertySymbol));
						break;
					case IEventSymbol eventSymbol:
						model.Events.Add(_memberBuilder.BuildEventModel(eventSymbol));
						break;
				}
			}
			catch
			{
				continue; // Skip problematic members
			}
		}
	}

	private bool IsRegularMethod(IMethodSymbol methodSymbol)
	{
		return methodSymbol.MethodKind == MethodKind.Ordinary ||
			   methodSymbol.MethodKind == MethodKind.ExplicitInterfaceImplementation;
	}

	private string GetTypeParameterConstraints(ITypeParameterSymbol typeParameter)
	{
		var constraints = new List<string>();

		if (typeParameter.HasReferenceTypeConstraint) constraints.Add("class");
		if (typeParameter.HasValueTypeConstraint) constraints.Add("struct");
		if (typeParameter.HasNotNullConstraint) constraints.Add("notnull");
		if (typeParameter.HasUnmanagedTypeConstraint) constraints.Add("unmanaged");
		if (typeParameter.HasConstructorConstraint) constraints.Add("new()");

		constraints.AddRange(typeParameter.ConstraintTypes
			.Where(t => t.TypeKind != TypeKind.Error)
			.Select(t => t.ToDisplayString()));

		return constraints.Any()
			? $"where {typeParameter.Name} : {string.Join(", ", constraints)}"
			: string.Empty;
	}
}
