using System;
using System.Linq;
using System.Text;
using DynaMock.SourceGeneration.Models;

namespace DynaMock.SourceGeneration;

/// <summary>
/// Helper for generating individual members
/// </summary>
public class CodeGeneratorHelper
{
	public void GenerateMethod(StringBuilder builder, MethodModel method, bool isInterface, bool virtualMembers)
	{
		var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
		var arguments = string.Join(", ", method.Parameters.Select(p => p.Name));
		var modifiers = GetMethodModifiers(method, isInterface, virtualMembers);
		var typeParams = method.TypeParameters.Any() ? $"<{string.Join(", ", method.TypeParameters)}>" : "";

		builder.AppendLine($"        {modifiers}{method.ReturnType} {method.Name}{typeParams}({parameters})");

		foreach (var constraint in method.Constraints)
			builder.AppendLine($"            {constraint}");

		builder.AppendLine("        {");
		GenerateMethodBody(builder, method, arguments, typeParams);
		builder.AppendLine("        }");
		builder.AppendLine();
	}

	public void GenerateProperty(StringBuilder builder, PropertyModel property, bool isInterface, bool virtualMembers)
	{
		var modifiers = GetPropertyModifiers(property, isInterface, virtualMembers);

		builder.AppendLine($"        {modifiers}{property.Type} {property.Name}");
		builder.AppendLine("        {");

		if (property.HasGetter)
			GeneratePropertyGetter(builder, property);

		if (property.HasSetter)
			GeneratePropertySetter(builder, property);

		builder.AppendLine("        }");
		builder.AppendLine();
	}

	public void GenerateEvent(StringBuilder builder, EventModel evt, bool isInterface, bool virtualMembers)
	{
		var modifiers = GetEventModifiers(evt, isInterface, virtualMembers);

		builder.AppendLine($"        {modifiers}event {evt.Type} {evt.Name}");
		builder.AppendLine("        {");

		GenerateEventAccessor(builder, evt, "add", "+=");
		GenerateEventAccessor(builder, evt, "remove", "-=");

		builder.AppendLine("        }");
		builder.AppendLine();
	}

	private void GenerateMethodBody(StringBuilder builder, MethodModel method, string arguments, string typeParams)
	{
		bool isVoid = method.ReturnType == "void";

		builder.AppendLine($"            if (ShouldUseMockForMethod(\"{method.Name}\"))");
		builder.AppendLine("            {");

		if (isVoid)
		{
			builder.AppendLine($"                MockProvider.Current.{method.Name}{typeParams}({arguments});");
			builder.AppendLine("                return;");
		}
		else
		{
			builder.AppendLine($"                return MockProvider.Current.{method.Name}{typeParams}({arguments});");
		}

		builder.AppendLine("            }");
		builder.AppendLine();

		if (isVoid)
			builder.AppendLine($"            RealImplementation.{method.Name}{typeParams}({arguments});");
		else
			builder.AppendLine($"            return RealImplementation.{method.Name}{typeParams}({arguments});");
	}

	private void GeneratePropertyGetter(StringBuilder builder, PropertyModel property)
	{
		builder.AppendLine("            get");
		builder.AppendLine("            {");
		builder.AppendLine($"                if (ShouldUseMockForProperty(\"{property.Name}\"))");
		builder.AppendLine($"                    return MockProvider.Current.{property.Name};");
		builder.AppendLine();
		builder.AppendLine($"                return RealImplementation.{property.Name};");
		builder.AppendLine("            }");
	}

	private void GeneratePropertySetter(StringBuilder builder, PropertyModel property)
	{
		builder.AppendLine("            set");
		builder.AppendLine("            {");
		builder.AppendLine($"                if (ShouldUseMockForProperty(\"{property.Name}\"))");
		builder.AppendLine("                {");
		builder.AppendLine($"                    MockProvider.Current.{property.Name} = value;");
		builder.AppendLine("                    return;");
		builder.AppendLine("                }");
		builder.AppendLine();
		builder.AppendLine($"                RealImplementation.{property.Name} = value;");
		builder.AppendLine("            }");
	}

	private void GenerateEventAccessor(StringBuilder builder, EventModel evt, string accessor, string op)
	{
		builder.AppendLine($"            {accessor}");
		builder.AppendLine("            {");
		builder.AppendLine($"                if (ShouldUseMockForEvent(\"{evt.Name}\"))");
		builder.AppendLine("                {");
		builder.AppendLine($"                    MockProvider.Current.{evt.Name} {op} value;");
		builder.AppendLine("                    return;");
		builder.AppendLine("                }");
		builder.AppendLine($"                RealImplementation.{evt.Name} {op} value;");
		builder.AppendLine("            }");
	}

	private string GetMethodModifiers(MethodModel method, bool isInterface, bool virtualMembers)
	{
		if (isInterface) return "public ";

		var modifiers = "public ";
		if (method.IsVirtual || method.IsAbstract)
			modifiers += "override ";
		else if (virtualMembers)
			modifiers += "virtual ";

		return modifiers;
	}

	private string GetPropertyModifiers(PropertyModel property, bool isInterface, bool virtualMembers)
	{
		if (isInterface) return "public ";

		var modifiers = "public ";
		if (property.IsVirtual || property.IsAbstract)
			modifiers += "override ";
		else if (virtualMembers)
			modifiers += "virtual ";

		return modifiers;
	}

	private string GetEventModifiers(EventModel evt, bool isInterface, bool virtualMembers)
	{
		if (isInterface) return "public ";

		var modifiers = "public ";
		if (evt.IsVirtual || evt.IsAbstract)
			modifiers += "override ";
		else if (virtualMembers)
			modifiers += "virtual ";

		return modifiers;
	}
}