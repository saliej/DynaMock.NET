using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using DynaMock.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DynaMock.SourceGeneration;

[Generator]
public class MockableGenerator : IIncrementalGenerator
{
    private const string MockableAttributeFullName = "DynaMock.MockableAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Create an incremental value provider for types with MockableAttribute
        var mockableTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                MockableAttributeFullName,
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (context, cancellationToken) => GetTypesToMock(context, cancellationToken))
            .SelectMany(static (typesToMock, _) => typesToMock);

        // Register source output
        context.RegisterSourceOutput(mockableTypes, static (context, typeToMock) =>
        {
            try
            {
                GenerateSource(context, typeToMock);
            }
            catch (Exception ex)
            {
                // Report diagnostic for generation errors
                var diagnostic = Diagnostic.Create(
                    GenerationErrorDescriptor,
                    Location.None,
                    typeToMock.TypeSymbol.Name,
                    ex.Message);
                context.ReportDiagnostic(diagnostic);
            }
        });
    }

    private static readonly DiagnosticDescriptor GenerationErrorDescriptor = new(
        "MOCK001",
        "Error generating mockable wrapper",
        "Failed to generate mockable wrapper for '{0}': {1}",
        "DynaMock",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnsupportedTypeDescriptor = new(
        "MOCK002",
        "Unsupported type for mocking",
        "Type '{0}' cannot be mocked: {1}",
        "DynaMock",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static ImmutableArray<TypeToMock> GetTypesToMock(
        GeneratorAttributeSyntaxContext context, 
        CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol) 
            return ImmutableArray<TypeToMock>.Empty;

        var attribute = context.Attributes
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == MockableAttributeFullName);
        
        if (attribute is null) return ImmutableArray<TypeToMock>.Empty;

        var typesToMock = ImmutableArray.CreateBuilder<TypeToMock>();

        // Handle constructor arguments - support both single Type and params Type[]
        if (attribute.ConstructorArguments.Length > 0)
        {
            var firstArg = attribute.ConstructorArguments[0];
            
            if (firstArg.Kind == TypedConstantKind.Array)
            {
                // params Type[] case
                foreach (var value in firstArg.Values)
                {
                    if (value.Value is INamedTypeSymbol namedType)
                    {
                        typesToMock.Add(new TypeToMock(namedType, context.TargetNode.GetLocation()));
                    }
                }
            }
            else if (firstArg.Value is INamedTypeSymbol singleType)
            {
                // Single Type case
                typesToMock.Add(new TypeToMock(singleType, context.TargetNode.GetLocation()));
            }
        }

        return typesToMock.ToImmutable();
    }

    private static void GenerateSource(SourceProductionContext context, TypeToMock typeToMock)
    {
        var typeSymbol = typeToMock.TypeSymbol;

        // Validate the type can be mocked
        if (!CanBeMocked(typeSymbol, out var reason))
        {
            var diagnostic = Diagnostic.Create(
                UnsupportedTypeDescriptor,
                typeToMock.Location,
                typeSymbol.Name,
                reason);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        var typeModel = BuildTypeModel(typeSymbol);
        var sourceCode = GenerateWrapperClass(typeModel);
        var fileName = $"Mockable{typeSymbol.Name}.g.cs";

        context.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
    }

    private static bool CanBeMocked(INamedTypeSymbol typeSymbol, out string reason)
    {
        reason = string.Empty;

        // Check if it's a sealed class
        if (typeSymbol.TypeKind == TypeKind.Class && typeSymbol.IsSealed)
        {
            reason = "sealed classes cannot be mocked";
            return false;
        }

        // Check if it's a static class
        if (typeSymbol.IsStatic)
        {
            reason = "static classes cannot be mocked";
            return false;
        }

        // Check if it's a primitive type
        if (typeSymbol.SpecialType != SpecialType.None && 
            typeSymbol.SpecialType != SpecialType.System_Object)
        {
            reason = "primitive types cannot be mocked";
            return false;
        }

        // Check if it's a delegate
        if (typeSymbol.TypeKind == TypeKind.Delegate)
        {
            reason = "delegates cannot be mocked";
            return false;
        }

        // Check if it's an enum
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            reason = "enums cannot be mocked";
            return false;
        }

        // Check if it's a struct (value types generally can't be mocked in this pattern)
        if (typeSymbol.TypeKind == TypeKind.Struct)
        {
            reason = "structs cannot be mocked";
            return false;
        }

        return true;
    }

    private static TypeModel BuildTypeModel(INamedTypeSymbol typeSymbol)
    {
        var model = new TypeModel
        {
            Name = typeSymbol.Name,
            Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
            IsInterface = typeSymbol.TypeKind == TypeKind.Interface,
            GenericTypeParameters = typeSymbol.TypeParameters.Select(tp => tp.Name).ToList(),
            GenericTypeConstraints = typeSymbol.TypeParameters
                .Select(GetTypeParameterConstraints)
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList()
        };

        foreach (var member in typeSymbol.GetMembers())
        {
            switch (member)
            {
                case IMethodSymbol methodSymbol when IsRegularMethod(methodSymbol):
                    model.Methods.Add(BuildMethodModel(methodSymbol));
                    break;
                case IPropertySymbol propertySymbol:
                    model.Properties.Add(BuildPropertyModel(propertySymbol));
                    break;
                case IEventSymbol eventSymbol:
                    model.Events.Add(BuildEventModel(eventSymbol));
                    break;
            }
        }

        return model;
    }

    private static bool IsRegularMethod(IMethodSymbol methodSymbol)
    {
        return methodSymbol.MethodKind == MethodKind.Ordinary ||
               methodSymbol.MethodKind == MethodKind.ExplicitInterfaceImplementation;
    }

    private static MethodModel BuildMethodModel(IMethodSymbol methodSymbol)
    {
        var baseImplementation = !methodSymbol.IsAbstract && !methodSymbol.IsVirtual
            ? $"base.{methodSymbol.Name}({string.Join(", ", methodSymbol.Parameters.Select(p => p.Name))})"
            : null;

        return new MethodModel
        {
            Name = methodSymbol.Name,
            ReturnType = methodSymbol.ReturnType.ToDisplayString(),
            Parameters = methodSymbol.Parameters
                .Select(p => (p.Type.ToDisplayString(), p.Name))
                .ToList(),
            IsAsync = IsAsyncMethod(methodSymbol),
            IsVirtual = methodSymbol.IsVirtual,
            IsAbstract = methodSymbol.IsAbstract,
            BaseImplementation = baseImplementation,
            TypeParameters = methodSymbol.TypeParameters
                .Select(tp => tp.Name)
                .ToList(),
            Constraints = methodSymbol.TypeParameters
                .Select(GetTypeParameterConstraints)
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList(),
            HasImplementation = !methodSymbol.IsAbstract && 
                               methodSymbol.ContainingType.TypeKind != TypeKind.Interface
        };
    }

    private static bool IsAsyncMethod(IMethodSymbol methodSymbol)
    {
        var returnType = methodSymbol.ReturnType;
        return returnType.Name is "Task" or "ValueTask" ||
               (returnType is INamedTypeSymbol { IsGenericType: true } namedType &&
                namedType.ConstructedFrom.Name is "Task" or "ValueTask");
    }

    private static PropertyModel BuildPropertyModel(IPropertySymbol propertySymbol)
    {
        return new PropertyModel
        {
            Name = propertySymbol.Name,
            Type = propertySymbol.Type.ToDisplayString(),
            HasGetter = propertySymbol.GetMethod != null,
            HasSetter = propertySymbol.SetMethod != null,
            IsVirtual = propertySymbol.IsVirtual,
            IsAbstract = propertySymbol.IsAbstract,
            GetterBaseImplementation = propertySymbol.GetMethod != null && 
                                     !propertySymbol.IsAbstract && 
                                     !propertySymbol.IsVirtual
                ? $"base.{propertySymbol.Name}"
                : null,
            SetterBaseImplementation = propertySymbol.SetMethod != null && 
                                     !propertySymbol.IsAbstract && 
                                     !propertySymbol.IsVirtual
                ? $"base.{propertySymbol.Name} = value"
                : null
        };
    }

    private static EventModel BuildEventModel(IEventSymbol eventSymbol)
    {
        return new EventModel
        {
            Name = eventSymbol.Name,
            Type = eventSymbol.Type.ToDisplayString(),
            IsVirtual = eventSymbol.IsVirtual,
            IsAbstract = eventSymbol.IsAbstract
        };
    }

    private static string GetTypeParameterConstraints(ITypeParameterSymbol typeParameter)
    {
        var constraints = new List<string>();

        if (typeParameter.HasReferenceTypeConstraint)
            constraints.Add("class");
        if (typeParameter.HasValueTypeConstraint)
            constraints.Add("struct");
        if (typeParameter.HasNotNullConstraint)
            constraints.Add("notnull");
        if (typeParameter.HasUnmanagedTypeConstraint)
            constraints.Add("unmanaged");
        if (typeParameter.HasConstructorConstraint)
            constraints.Add("new()");

        constraints.AddRange(typeParameter.ConstraintTypes
            .Where(t => t.TypeKind != TypeKind.Error)
            .Select(t => t.ToDisplayString()));

        return constraints.Any() 
            ? $"where {typeParameter.Name} : {string.Join(", ", constraints)}" 
            : string.Empty;
    }

    private static string GenerateWrapperClass(TypeModel model)
    {
        var builder = new StringBuilder();

        // Add using statements
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Threading;");
        builder.AppendLine("using System.Threading.Tasks;");
        
        // Only add namespace using if it's different from our generated namespace
        if (!string.IsNullOrEmpty(model.Namespace) && model.Namespace != "DynaMock.Generated")
        {
            builder.AppendLine($"using {model.Namespace};");
        }
        
        builder.AppendLine();

        // Add namespace
        builder.AppendLine("namespace DynaMock.Generated");
        builder.AppendLine("{");

        // Begin class declaration
        var genericTypeParams = model.GenericTypeParameters.Any() 
            ? $"<{string.Join(", ", model.GenericTypeParameters)}>"
            : "";
        var baseType = $"MockableBase<{model.Name}{genericTypeParams}>";
        var interfaces = model.IsInterface ? $", {model.Name}{genericTypeParams}" : "";

        builder.AppendLine($"    public class Mockable{model.Name}{genericTypeParams} : {baseType}{interfaces}");

        // Add generic type constraints if any
        foreach (var constraint in model.GenericTypeConstraints)
        {
            builder.AppendLine($"        {constraint}");
        }

        builder.AppendLine("    {");

        // Add constructor
        builder.AppendLine($"        public Mockable{model.Name}({model.Name}{genericTypeParams} realImplementation)");
        builder.AppendLine("            : base(realImplementation)");
        builder.AppendLine("        {");
        builder.AppendLine("        }");
        builder.AppendLine();

        // Add method implementations
        foreach (var method in model.Methods)
        {
            GenerateMethod(builder, method, model.IsInterface);
        }

        // Add property implementations
        foreach (var property in model.Properties)
        {
            GenerateProperty(builder, property, model.IsInterface);
        }

        // Add event implementations
        foreach (var evt in model.Events)
        {
            GenerateEvent(builder, evt, model.IsInterface);
        }

        // Close class and namespace
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static void GenerateMethod(StringBuilder builder, MethodModel method, bool isInterface)
    {
        var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
        var arguments = string.Join(", ", method.Parameters.Select(p => p.Name));
        var @override = !isInterface && (method.IsVirtual || method.IsAbstract) ? "override " : "";
        var typeParams = method.TypeParameters.Any()
            ? $"<{string.Join(", ", method.TypeParameters)}>"
            : "";

        builder.AppendLine($"        public {@override}{method.ReturnType} {method.Name}{typeParams}({parameters})");
    
        // Add type constraints if any
        foreach (var constraint in method.Constraints)
        {
            builder.AppendLine($"            {constraint}");
        }

        builder.AppendLine("        {");
    
        bool isVoidMethod = method.ReturnType == "void";
    
        // Generate conditional logic for partial mocking
        builder.AppendLine($"            if (ShouldUseMockForMethod(\"{method.Name}\"))");
        if (isVoidMethod)
        {
            builder.AppendLine($"            {{");
            builder.AppendLine($"                Mock.{method.Name}({arguments});");
            builder.AppendLine($"                return;");
            builder.AppendLine($"            }}");
        }
        else
        {
            builder.AppendLine($"                return Mock.{method.Name}({arguments});");
        }
    
        if (method.BaseImplementation != null)
        {
            if (isVoidMethod)
            {
                // Extract just the method call part without return for void methods
                var methodCall = method.BaseImplementation.Replace("return ", "");
                builder.AppendLine($"            {methodCall};");
            }
            else
            {
                builder.AppendLine($"            return {method.BaseImplementation};");
            }
        }
        else
        {
            if (isVoidMethod)
            {
                builder.AppendLine($"            _realImplementation.{method.Name}({arguments});");
            }
            else
            {
                builder.AppendLine($"            return _realImplementation.{method.Name}({arguments});");
            }
        }
    
        builder.AppendLine("        }");
        builder.AppendLine();
    }

    private static void GenerateProperty(StringBuilder builder, PropertyModel property, bool isInterface)
    {
        var @override = !isInterface && (property.IsVirtual || property.IsAbstract) ? "override " : "";
    
        builder.AppendLine($"        public {@override}{property.Type} {property.Name}");
        builder.AppendLine("        {");
    
        if (property.HasGetter)
        {
            builder.AppendLine("            get");
            builder.AppendLine("            {");
            builder.AppendLine($"                if (ShouldUseMockForProperty(\"{property.Name}\"))");
            builder.AppendLine($"                    return Mock.{property.Name};");
        
            if (property.GetterBaseImplementation != null)
            {
                builder.AppendLine($"                return {property.GetterBaseImplementation};");
            }
            else
            {
                builder.AppendLine($"                return _realImplementation.{property.Name};");
            }
            builder.AppendLine("            }");
        }
    
        if (property.HasSetter)
        {
            builder.AppendLine("            set");
            builder.AppendLine("            {");
            builder.AppendLine($"                if (ShouldUseMockForProperty(\"{property.Name}\"))");
            builder.AppendLine($"                    Mock.{property.Name} = value;");
            builder.AppendLine("                else");
        
            if (property.SetterBaseImplementation != null)
            {
                builder.AppendLine($"                    {property.SetterBaseImplementation};");
            }
            else
            {
                builder.AppendLine($"                    _realImplementation.{property.Name} = value;");
            }
            builder.AppendLine("            }");
        }
    
        builder.AppendLine("        }");
        builder.AppendLine();
    }

    private static void GenerateEvent(StringBuilder builder, EventModel evt, bool isInterface)
    {
        var @override = !isInterface && (evt.IsVirtual || evt.IsAbstract) ? "override " : "";

        builder.AppendLine($"        public {@override}event {evt.Type} {evt.Name}");
        builder.AppendLine("        {");
        builder.AppendLine($"            add => Implementation.{evt.Name} += value;");
        builder.AppendLine($"            remove => Implementation.{evt.Name} -= value;");
        builder.AppendLine("        }");
        builder.AppendLine();
    }
}

// Helper struct to carry type information through the incremental pipeline
internal struct TypeToMock
{
    public TypeToMock(INamedTypeSymbol typeSymbol, Location location)
    {
        TypeSymbol = typeSymbol ?? throw new ArgumentNullException(nameof(typeSymbol));
        Location = location ?? throw new ArgumentNullException(nameof(location));
    }

    public INamedTypeSymbol TypeSymbol { get; set; }
    public Location Location { get; set; }
} 