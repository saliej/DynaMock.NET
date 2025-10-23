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
        var methodArgs = string.IsNullOrEmpty(arguments) ? "" : arguments;
    
        // Create array of arguments for matching
        var argArray = method.Parameters.Any() 
            ? $"new object?[] {{ {string.Join(", ", method.Parameters.Select(p => p.Name))} }}"
            : "Array.Empty<object?>()";

        if (isVoid)
        {
            builder.AppendLine($"            Interceptor.InterceptVoidMethod(");
            builder.AppendLine($"                x => x.{method.Name}{typeParams}({methodArgs}),");
            builder.AppendLine($"                impl => impl.{method.Name}{typeParams}({methodArgs}),");
            builder.AppendLine($"                {argArray});");
        }
        else
        {
            builder.AppendLine($"            return Interceptor.InterceptMethod(");
            builder.AppendLine($"                x => x.{method.Name}{typeParams}({methodArgs}),");
            builder.AppendLine($"                impl => impl.{method.Name}{typeParams}({methodArgs}),");
            builder.AppendLine($"                {argArray});");
        }
    }

    private void GeneratePropertyGetter(StringBuilder builder, PropertyModel property)
    {
        builder.AppendLine("            get");
        builder.AppendLine("            {");
        builder.AppendLine($"                return Interceptor.InterceptPropertyGet(");
        builder.AppendLine($"                    x => x.{property.Name},");
        builder.AppendLine($"                    impl => impl.{property.Name});");
        builder.AppendLine("            }");
    }

    private void GeneratePropertySetter(StringBuilder builder, PropertyModel property)
    {
        builder.AppendLine("            set");
        builder.AppendLine("            {");
        builder.AppendLine($"                Interceptor.InterceptPropertySet(");
        builder.AppendLine($"                    x => x.{property.Name},");
        builder.AppendLine($"                    impl => impl.{property.Name} = value,");
        builder.AppendLine($"                    value);");
        builder.AppendLine("            }");
    }

    private void GenerateEventAccessor(StringBuilder builder, EventModel evt, string accessor, string op)
    {
        var methodName = accessor == "add" ? "InterceptEventAdd" : "InterceptEventRemove";
    
        builder.AppendLine($"            {accessor}");
        builder.AppendLine("            {");
        builder.AppendLine($"                Interceptor.{methodName}(");
        builder.AppendLine($"                    \"{evt.Name}\",");
        builder.AppendLine($"                    impl => impl.{evt.Name} {op} value);");
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
		else modifiers += "virtual ";

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