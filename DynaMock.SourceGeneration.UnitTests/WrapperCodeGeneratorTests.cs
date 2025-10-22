using System;
using AwesomeAssertions;
using DynaMock.SourceGeneration.Models;

namespace DynaMock.SourceGeneration.UnitTests;

public class WrapperCodeGeneratorTests
{
	private readonly WrapperCodeGenerator _generator;

	public WrapperCodeGeneratorTests()
	{
		_generator = new WrapperCodeGenerator();
	}

	[Fact]
	public void GenerateWrapperClass_WithSimpleInterface_GeneratesValidCode()
	{
		var model = new TypeModel
		{
			Name = "ITestService",
			Namespace = "Test.Namespace",
			IsInterface = true,
			Methods =
			[
				new MethodModel
				{
					Name = "GetValue",
					ReturnType = "string",
					Parameters = [],
					TypeParameters = [],
					Constraints = []
				}
			],
			Properties = [],
			Events = [],
			GenericTypeParameters = [],
			GenericTypeConstraints = []
		};

		var result = _generator.GenerateWrapperClass(model, virtualMembers: false);

		result.Should().Contain("using System;");
		result.Should().Contain("using DynaMock;");
		result.Should().Contain("using Test.Namespace;");
		result.Should().Contain("namespace DynaMock.Generated");
		result.Should().Contain("public class MockableITestService : MockableBase<ITestService>, ITestService");
		result.Should().Contain("public MockableITestService(ITestService realImplementation)");
		result.Should().Contain("public string GetValue()");
	}

	[Fact]
	public void GenerateWrapperClass_WithGenericType_IncludesTypeParameters()
	{
		var model = new TypeModel
		{
			Name = "IRepository",
			Namespace = "Test.Namespace",
			IsInterface = true,
			GenericTypeParameters = ["T"],
			GenericTypeConstraints = ["where T : class"],
			Methods = [],
			Properties = [],
			Events = []
		};

		var result = _generator.GenerateWrapperClass(model, virtualMembers: false);

		result.Should().Contain("public class MockableIRepository<T> : MockableBase<IRepository<T>>, IRepository<T>");
		result.Should().Contain("where T : class");
	}

	[Fact]
	public void GenerateWrapperClass_WithClass_DoesNotImplementInterface()
	{
		var model = new TypeModel
		{
			Name = "TestService",
			Namespace = "Test.Namespace",
			IsInterface = false,
			Methods = [],
			Properties = [],
			Events = [],
			GenericTypeParameters = [],
			GenericTypeConstraints = []
		};

		var result = _generator.GenerateWrapperClass(model, virtualMembers: false);

		result.Should().Contain("public class MockableTestService : MockableBase<TestService>");
		result.Should().NotContain(", TestService"); // Should not have interface implementation
	}

	[Fact]
	public void GenerateWrapperClass_WithPropertiesAndEvents_IncludesAllMembers()
	{
		var model = new TypeModel
		{
			Name = "ITestService",
			Namespace = "Test.Namespace",
			IsInterface = true,
			Methods =
			[
				new MethodModel
				{
					Name = "GetValue",
					ReturnType = "string",
					Parameters = [],
					TypeParameters = [],
					Constraints = []
				}
			],
			Properties =
			[
				new PropertyModel { Name = "Name", Type = "string", HasGetter = true, HasSetter = true }
			],
			Events =
			[
				new EventModel { Name = "Changed", Type = "EventHandler" }
			],
			GenericTypeParameters = [],
			GenericTypeConstraints = []
		};

		var result = _generator.GenerateWrapperClass(model, virtualMembers: false);

		result.Should().Contain("public string GetValue()");
		result.Should().Contain("public string Name");
		result.Should().Contain("public event EventHandler Changed");
	}

	[Fact]
	public void GenerateWrapperClass_WithGlobalNamespace_DoesNotAddUsingForNamespace()
	{
		var model = new TypeModel
		{
			Name = "ITestService",
			Namespace = string.Empty, // Global namespace
			IsInterface = true,
			Methods = [],
			Properties = [],
			Events = [],
			GenericTypeParameters = [],
			GenericTypeConstraints = []
		};

		var result = _generator.GenerateWrapperClass(model, virtualMembers: false);

		result.Should().NotContain("using ;"); // Should not have empty using statement
	}
}