using System;
using System.Linq;
using System.Text;
using DynaMock.SourceGeneration.Models;

namespace DynaMock.SourceGeneration;

/// <summary>
/// Generates base class source code for mockable wrappers
/// </summary>
public class BaseClassCodeGenerator
{
	public string GenerateBaseClass(TypeModel model, bool virtualMembers)
	{
		var builder = new StringBuilder();
		GenerateUsings(builder, model);
		GenerateNamespace(builder, model);

		return builder.ToString().Replace("\r\n", "\n");
	}

	private void GenerateUsings(StringBuilder builder, TypeModel model)
	{
		builder.AppendLine("using System;");
		builder.AppendLine("using DynaMock;");

		if (!string.IsNullOrEmpty(model.Namespace) && model.Namespace != "DynaMock.Generated")
			builder.AppendLine($"using {model.Namespace};");

		builder.AppendLine();
	}

	private void GenerateNamespace(StringBuilder builder, TypeModel model)
	{
		builder.AppendLine("namespace DynaMock.Generated");
		builder.AppendLine("{");

		GenerateClass(builder, model);

		builder.AppendLine("}");
	}

	private void GenerateClass(StringBuilder builder, TypeModel model)
	{
		var genericParams = model.GenericTypeParameters.Any()
			? $"<{string.Join(", ", model.GenericTypeParameters)}>"
			: "";

		// Key: Inherit from T if it's a class, nothing if interface
		string baseClass = "";
		if (!model.IsInterface)
		{
			baseClass = $" : {model.Name}{genericParams}";
		}

		builder.AppendLine($"    public abstract class Mockable{model.Name}Base{genericParams}{baseClass}");

		foreach (var constraint in model.GenericTypeConstraints)
			builder.AppendLine($"        {constraint}");

		builder.AppendLine("    {");

		GenerateFields(builder, model, genericParams);
		GenerateConstructor(builder, model, genericParams);
		GenerateHelperMembers(builder, model, genericParams);

		builder.AppendLine("    }");
	}

	private void GenerateFields(StringBuilder builder, TypeModel model, string genericParams)
	{
		builder.AppendLine($"        protected readonly {model.Name}{genericParams} RealImplementation;");
		builder.AppendLine($"        protected readonly IMockProvider<{model.Name}{genericParams}> MockProvider;");
		builder.AppendLine();
	}

	private void GenerateConstructor(StringBuilder builder, TypeModel model, string genericParams)
	{
		var baseCall = model.IsInterface ? "" : "\n            : base()";

		builder.AppendLine($"        protected Mockable{model.Name}Base({model.Name}{genericParams} realImplementation, IMockProvider<{model.Name}{genericParams}>? mockProvider){baseCall}");
		builder.AppendLine("        {");
		builder.AppendLine("            RealImplementation = realImplementation ?? throw new ArgumentNullException(nameof(realImplementation));");
		builder.AppendLine($"            MockProvider = mockProvider ?? new DefaultMockProvider<{model.Name}{genericParams}>();");
		builder.AppendLine("        }");
		builder.AppendLine();
	}

	private void GenerateHelperMembers(StringBuilder builder, TypeModel model, string genericParams)
	{
		builder.AppendLine($"        protected {model.Name}{genericParams} Implementation => MockProvider.Current ?? RealImplementation;");
		builder.AppendLine();

		builder.AppendLine("        protected bool ShouldUseMockForMethod(string methodName)");
		builder.AppendLine("        {");
		builder.AppendLine("            return MockProvider.Current != null &&");
		builder.AppendLine("            (");
		builder.AppendLine("                MockProvider.MockConfig == null ||");
		builder.AppendLine("                MockProvider.MockConfig?.IsMethodMocked(methodName) == true");
		builder.AppendLine("            );");
		builder.AppendLine("        }");
		builder.AppendLine();

		builder.AppendLine("        protected bool ShouldUseMockForProperty(string propertyName)");
		builder.AppendLine("        {");
		builder.AppendLine("            return MockProvider.Current != null &&");
		builder.AppendLine("            (");
		builder.AppendLine("                MockProvider.MockConfig == null ||");
		builder.AppendLine("                MockProvider.MockConfig?.IsPropertyMocked(propertyName) == true");
		builder.AppendLine("            );");
		builder.AppendLine("        }");
		builder.AppendLine();

		builder.AppendLine("        protected bool ShouldUseMockForEvent(string eventName)");
		builder.AppendLine("        {");
		builder.AppendLine("            return MockProvider.Current != null &&");
		builder.AppendLine("            (");
		builder.AppendLine("                MockProvider.MockConfig == null ||");
		builder.AppendLine("                MockProvider.MockConfig?.IsEventMocked(eventName) == true");
		builder.AppendLine("            );");
		builder.AppendLine("        }");
		builder.AppendLine();

		builder.AppendLine($"        public {model.Name}{genericParams} GetRealImplementation() => RealImplementation;");
	}
}