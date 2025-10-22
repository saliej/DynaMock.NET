using System;
using System.Linq;
using System.Text;
using DynaMock.SourceGeneration.Models;


namespace DynaMock.SourceGeneration;

/// <summary>
/// Generates wrapper class source code
/// </summary>
public class WrapperCodeGenerator
{
	private readonly CodeGeneratorHelper _helper;

	public WrapperCodeGenerator()
	{
		_helper = new CodeGeneratorHelper();
	}

	public string GenerateWrapperClass(TypeModel model, bool virtualMembers)
	{
		var builder = new StringBuilder();

		GenerateUsings(builder, model);
		GenerateNamespace(builder, model, virtualMembers);

		return builder.ToString();
	}

	private void GenerateUsings(StringBuilder builder, TypeModel model)
	{
		builder.AppendLine("using System;");
		builder.AppendLine("using System.Threading;");
		builder.AppendLine("using System.Threading.Tasks;");
		builder.AppendLine("using DynaMock;");

		if (!string.IsNullOrEmpty(model.Namespace) && model.Namespace != "DynaMock.Generated")
			builder.AppendLine($"using {model.Namespace};");

		builder.AppendLine();
	}

	private void GenerateNamespace(StringBuilder builder, TypeModel model, bool virtualMembers)
	{
		builder.AppendLine("namespace DynaMock.Generated");
		builder.AppendLine("{");

		GenerateClass(builder, model, virtualMembers);

		builder.AppendLine("}");
	}

	private void GenerateClass(StringBuilder builder, TypeModel model, bool virtualMembers)
	{
		var genericParams = model.GenericTypeParameters.Any()
			? $"<{string.Join(", ", model.GenericTypeParameters)}>"
			: "";
		var baseType = $"MockableBase<{model.Name}{genericParams}>";
		var interfaces = model.IsInterface ? $", {model.Name}{genericParams}" : "";

		builder.AppendLine($"    public class Mockable{model.Name}{genericParams} : {baseType}{interfaces}");

		foreach (var constraint in model.GenericTypeConstraints)
			builder.AppendLine($"        {constraint}");

		builder.AppendLine("    {");

		GenerateConstructors(builder, model);
		GenerateMembers(builder, model, virtualMembers);

		builder.AppendLine("    }");
	}

	private void GenerateConstructors(StringBuilder builder, TypeModel model)
	{
		var genericParams = model.GenericTypeParameters.Any()
			? $"<{string.Join(", ", model.GenericTypeParameters)}>"
			: "";
		var className = $"Mockable{model.Name}";

		builder.AppendLine($"        public {className}({model.Name}{genericParams} realImplementation)");
		builder.AppendLine($"            : this(realImplementation, null)");
		builder.AppendLine("        {");
		builder.AppendLine("        }");
		builder.AppendLine();

		builder.AppendLine($"        public {className}({model.Name}{genericParams} realImplementation, IMockProvider<{model.Name}{genericParams}>? mockProvider)");
		builder.AppendLine("            : base(realImplementation, mockProvider)");
		builder.AppendLine("        {");
		builder.AppendLine("        }");
		builder.AppendLine();
	}

	private void GenerateMembers(StringBuilder builder, TypeModel model, bool virtualMembers)
	{
		foreach (var method in model.Methods)
			_helper.GenerateMethod(builder, method, model.IsInterface, virtualMembers);

		foreach (var property in model.Properties)
			_helper.GenerateProperty(builder, property, model.IsInterface, virtualMembers);

		foreach (var evt in model.Events)
			_helper.GenerateEvent(builder, evt, model.IsInterface, virtualMembers);
	}
}