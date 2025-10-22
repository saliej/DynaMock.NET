using System;
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

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var mockableTypes = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				MockableAttributeFullName,
				predicate: static (node, _) => node is TypeDeclarationSyntax,
				transform: static (context, cancellationToken) => GetTypesToMock(context, cancellationToken))
			.SelectMany(static (typesToMock, _) => typesToMock);

		context.RegisterSourceOutput(mockableTypes, static (context, typeToMock) =>
		{
			try
			{
				GenerateSource(context, typeToMock);
			}
			catch (Exception ex)
			{
				var diagnostic = Diagnostic.Create(
					GenerationErrorDescriptor,
					Location.None,
					typeToMock.TypeSymbol.Name,
					ex.Message);
				context.ReportDiagnostic(diagnostic);
			}
		});
	}

	private static ImmutableArray<TypeToMock> GetTypesToMock(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken)
	{
		var extractor = new AttributeTypesExtractor();
		return extractor.ExtractTypesToMock(context, MockableAttributeFullName);
	}

	private static void GenerateSource(SourceProductionContext context, TypeToMock typeToMock)
	{
		var validator = new TypeValidator();
		var typeSymbol = typeToMock.TypeSymbol;

		if (!validator.CanBeMocked(typeSymbol, out var reason))
		{
			var diagnostic = Diagnostic.Create(
				UnsupportedTypeDescriptor,
				typeToMock.Location,
				typeSymbol.Name,
				reason);
			context.ReportDiagnostic(diagnostic);
			return;
		}

		var modelBuilder = new TypeModelBuilder();
		var typeModel = modelBuilder.BuildTypeModel(typeSymbol);

		var codeGenerator = new WrapperCodeGenerator();
		var sourceCode = codeGenerator.GenerateWrapperClass(typeModel, typeToMock.VirtualMembers);
		var fileName = $"Mockable{typeSymbol.Name}.g.cs";

		context.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
	}
}