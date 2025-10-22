using System;
using System.Text;
using AwesomeAssertions;
using DynaMock.SourceGeneration.Models;

namespace DynaMock.SourceGeneration.UnitTests;

public class CodeGeneratorHelperTests
{
	private readonly CodeGeneratorHelper _helper;

	public CodeGeneratorHelperTests()
	{
		_helper = new CodeGeneratorHelper();
	}

	[Fact]
	public void GenerateMethod_WithVoidMethod_GeneratesCorrectCode()
	{
		var builder = new StringBuilder();
		var method = new MethodModel
		{
			Name = "DoSomething",
			ReturnType = "void",
			Parameters = [],
			TypeParameters = [],
			Constraints = []
		};

		_helper.GenerateMethod(builder, method, isInterface: false, virtualMembers: false);
		var result = builder.ToString();

		result.Should().Contain("public void DoSomething()");
		result.Should().Contain("if (ShouldUseMockForMethod(\"DoSomething\"))");
		result.Should().Contain("MockProvider.Current.DoSomething()");
		result.Should().Contain("RealImplementation.DoSomething()");
	}

	[Fact]
	public void GenerateMethod_WithReturnType_GeneratesCorrectCode()
	{
		var builder = new StringBuilder();
		var method = new MethodModel
		{
			Name = "GetValue",
			ReturnType = "string",
			Parameters = [],
			TypeParameters = [],
			Constraints = []
		};

		_helper.GenerateMethod(builder, method, isInterface: false, virtualMembers: false);
		var result = builder.ToString();

		result.Should().Contain("public string GetValue()");
		result.Should().Contain("return MockProvider.Current.GetValue()");
		result.Should().Contain("return RealImplementation.GetValue()");
	}

	[Fact]
	public void GenerateMethod_WithParameters_IncludesParametersInSignature()
	{
		var builder = new StringBuilder();
		var method = new MethodModel
		{
			Name = "SetValue",
			ReturnType = "void",
			Parameters = [("string", "value"), ("int", "index")],
			TypeParameters = [],
			Constraints = []
		};

		_helper.GenerateMethod(builder, method, isInterface: false, virtualMembers: false);
		var result = builder.ToString();

		result.Should().Contain("SetValue(string value, int index)");
		result.Should().Contain("MockProvider.Current.SetValue(value, index)");
		result.Should().Contain("RealImplementation.SetValue(value, index)");
	}

	[Fact]
	public void GenerateMethod_WithVirtualMethod_IncludesOverrideModifier()
	{
		var builder = new StringBuilder();
		var method = new MethodModel
		{
			Name = "GetValue",
			ReturnType = "string",
			IsVirtual = true,
			Parameters = [],
			TypeParameters = [],
			Constraints = []
		};

		_helper.GenerateMethod(builder, method, isInterface: false, virtualMembers: false);
		var result = builder.ToString();

		result.Should().Contain("public override string GetValue()");
	}

	[Fact]
	public void GenerateMethod_WithVirtualMembersEnabled_IncludesVirtualModifier()
	{
		var builder = new StringBuilder();
		var method = new MethodModel
		{
			Name = "GetValue",
			ReturnType = "string",
			IsVirtual = false,
			Parameters = [],
			TypeParameters = [],
			Constraints = []
		};

		_helper.GenerateMethod(builder, method, isInterface: false, virtualMembers: true);
		var result = builder.ToString();

		result.Should().Contain("public virtual string GetValue()");
	}

	[Fact]
	public void GenerateProperty_WithGetterAndSetter_GeneratesCorrectCode()
	{
		var builder = new StringBuilder();
		var property = new PropertyModel
		{
			Name = "Name",
			Type = "string",
			HasGetter = true,
			HasSetter = true
		};

		_helper.GenerateProperty(builder, property, isInterface: false, virtualMembers: false);
		var result = builder.ToString();

		result.Should().Contain("public string Name");
		result.Should().Contain("get");
		result.Should().Contain("set");
		result.Should().Contain("if (ShouldUseMockForProperty(\"Name\"))");
		result.Should().Contain("MockProvider.Current.Name");
		result.Should().Contain("RealImplementation.Name");
	}

	[Fact]
	public void GenerateProperty_WithGetterOnly_DoesNotGenerateSetter()
	{
		var builder = new StringBuilder();
		var property = new PropertyModel
		{
			Name = "Id",
			Type = "int",
			HasGetter = true,
			HasSetter = false
		};

		_helper.GenerateProperty(builder, property, isInterface: false, virtualMembers: false);
		var result = builder.ToString();

		result.Should().Contain("get");
		result.Should().NotContain("set");
	}

	[Fact]
	public void GenerateEvent_GeneratesCorrectCode()
	{
		var builder = new StringBuilder();
		var evt = new EventModel
		{
			Name = "Changed",
			Type = "EventHandler"
		};

		_helper.GenerateEvent(builder, evt, isInterface: false, virtualMembers: false);
		var result = builder.ToString();

		result.Should().Contain("public event EventHandler Changed");
		result.Should().Contain("add");
		result.Should().Contain("remove");
		result.Should().Contain("if (ShouldUseMockForEvent(\"Changed\"))");
		result.Should().Contain("MockProvider.Current.Changed += value");
		result.Should().Contain("MockProvider.Current.Changed -= value");
		result.Should().Contain("RealImplementation.Changed += value");
		result.Should().Contain("RealImplementation.Changed -= value");
	}
}
