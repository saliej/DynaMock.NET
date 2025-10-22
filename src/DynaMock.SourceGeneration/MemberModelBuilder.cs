using System;
using System.Collections.Generic;
using System.Linq;
using DynaMock.SourceGeneration.Models;
using Microsoft.CodeAnalysis;


namespace DynaMock.SourceGeneration;

/// <summary>
/// Builds models for methods, properties, and events
/// </summary>
public class MemberModelBuilder
{
	public MethodModel BuildMethodModel(IMethodSymbol methodSymbol)
	{
		if (methodSymbol is null)
			throw new ArgumentNullException(nameof(methodSymbol));

		var baseImplementation = !methodSymbol.IsAbstract && !methodSymbol.IsVirtual
			? $"base.{methodSymbol.Name}({string.Join(", ", methodSymbol.Parameters.Select(p => p.Name))})"
			: null;

		return new MethodModel
		{
			Name = methodSymbol.Name,
			ReturnType = methodSymbol.ReturnType?.ToDisplayString() ?? "void",
			Parameters = methodSymbol.Parameters
				.Select(p => (p.Type?.ToDisplayString() ?? "object", p.Name ?? "arg"))
				.ToList(),
			IsAsync = IsAsyncMethod(methodSymbol),
			IsVirtual = methodSymbol.IsVirtual,
			IsAbstract = methodSymbol.IsAbstract,
			BaseImplementation = baseImplementation,
			TypeParameters = methodSymbol.TypeParameters.Select(tp => tp.Name).ToList(),
			Constraints = methodSymbol.TypeParameters
				.Select(GetTypeParameterConstraints)
				.Where(c => !string.IsNullOrEmpty(c))
				.ToList(),
			HasImplementation = !methodSymbol.IsAbstract &&
							   methodSymbol.ContainingType.TypeKind != TypeKind.Interface
		};
	}

	public PropertyModel BuildPropertyModel(IPropertySymbol propertySymbol)
	{
		if (propertySymbol is null)
			throw new ArgumentNullException(nameof(propertySymbol));

		return new PropertyModel
		{
			Name = propertySymbol.Name,
			Type = propertySymbol.Type?.ToDisplayString() ?? "object",
			HasGetter = propertySymbol.GetMethod != null,
			HasSetter = propertySymbol.SetMethod != null,
			IsVirtual = propertySymbol.IsVirtual,
			IsAbstract = propertySymbol.IsAbstract,
			GetterBaseImplementation = GetGetterBaseImplementation(propertySymbol),
			SetterBaseImplementation = GetSetterBaseImplementation(propertySymbol)
		};
	}

	public EventModel BuildEventModel(IEventSymbol eventSymbol)
	{
		if (eventSymbol is null)
			throw new ArgumentNullException(nameof(eventSymbol));

		return new EventModel
		{
			Name = eventSymbol.Name,
			Type = eventSymbol.Type?.ToDisplayString() ?? "EventHandler",
			IsVirtual = eventSymbol.IsVirtual,
			IsAbstract = eventSymbol.IsAbstract
		};
	}

	private bool IsAsyncMethod(IMethodSymbol methodSymbol)
	{
		var returnType = methodSymbol.ReturnType;
		return returnType.Name is "Task" or "ValueTask" ||
			   (returnType is INamedTypeSymbol { IsGenericType: true } namedType &&
				namedType.ConstructedFrom.Name is "Task" or "ValueTask");
	}

	private string? GetGetterBaseImplementation(IPropertySymbol propertySymbol)
	{
		return propertySymbol.GetMethod != null &&
			   !propertySymbol.IsAbstract &&
			   !propertySymbol.IsVirtual
			? $"base.{propertySymbol.Name}"
			: null;
	}

	private string? GetSetterBaseImplementation(IPropertySymbol propertySymbol)
	{
		return propertySymbol.SetMethod != null &&
			   !propertySymbol.IsAbstract &&
			   !propertySymbol.IsVirtual
			? $"base.{propertySymbol.Name} = value"
			: null;
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